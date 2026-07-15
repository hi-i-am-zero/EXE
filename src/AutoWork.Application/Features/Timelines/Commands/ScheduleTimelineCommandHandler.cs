using AutoWork.Application.Common.Exceptions;
using AutoWork.Application.DTOs.Posts;
using AutoWork.Application.DTOs.Timelines;
using AutoWork.Application.Features.Posts.Commands;
using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Application.Interfaces.Services;
using AutoWork.Domain.Entities;
using AutoWork.Domain.Enums;
using MediatR;

namespace AutoWork.Application.Features.Timelines.Commands;

/// <summary>Bước 5 (cuối) của luồng Chiến dịch tự động — nút "Đăng bài": xác nhận CẢ timeline,
/// tự tạo PostSchedule cho từng bài theo đúng ngày AI đã đề xuất. Bài đầu tiên (DayNumber=1) có thể
/// chọn đăng THẬT NGAY LẬP TỨC thay vì chỉ lên lịch — đúng theo mô tả luồng sử dụng.</summary>
public class ScheduleTimelineCommandHandler : IRequestHandler<ScheduleTimelineCommand, ScheduleTimelineResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<Timeline> _timelines;
    private readonly IRepository<Post> _posts;
    private readonly IRepository<PostSchedule> _postSchedules;
    private readonly IMediator _mediator;
    private readonly IProjectAccessGuard _accessGuard;
    private readonly ICurrentUserService _currentUser;

    public ScheduleTimelineCommandHandler(
        IUnitOfWork unitOfWork,
        IRepository<Timeline> timelines,
        IRepository<Post> posts,
        IRepository<PostSchedule> postSchedules,
        IMediator mediator,
        IProjectAccessGuard accessGuard,
        ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _timelines = timelines;
        _posts = posts;
        _postSchedules = postSchedules;
        _mediator = mediator;
        _accessGuard = accessGuard;
        _currentUser = currentUser;
    }

    public async Task<ScheduleTimelineResponse> Handle(ScheduleTimelineCommand command, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not Guid userId)
        {
            throw new UnauthorizedException("User is not authenticated.");
        }

        var timeline = await _timelines.GetByIdAsync(command.Request.TimelineId, cancellationToken)
            ?? throw new NotFoundException(nameof(Timeline), command.Request.TimelineId);

        await _accessGuard.EnsureEditorAsync(timeline.ProjectId, userId, cancellationToken);

        var posts = (await _posts.FindAsync(p => p.TimelineId == timeline.Id, cancellationToken))
            .OrderBy(p => p.DayNumber)
            .ToList();

        if (posts.Count == 0)
        {
            throw new BadRequestException("Timeline chưa có bài viết nào để lên lịch.");
        }

        var response = new ScheduleTimelineResponse { TimelineId = timeline.Id };
        var firstPost = posts[0];
        var remainingPosts = posts;

        if (command.Request.PublishFirstImmediately)
        {
            remainingPosts = posts.Skip(1).ToList();
            try
            {
                await _mediator.Send(new PublishPostCommand { PostId = firstPost.Id }, cancellationToken);
                response.FirstPublishedImmediately = true;
            }
            catch (Exception ex)
            {
                response.FirstPublishedImmediately = false;
                response.FirstPublishError = ex.Message;
                // vẫn lên lịch bình thường cho bài đầu nếu đăng ngay thất bại (VD: chưa kết nối Facebook)
                remainingPosts = posts;
            }
        }

        foreach (var post in remainingPosts)
        {
            await _postSchedules.AddAsync(new PostSchedule
            {
                PostId = post.Id,
                ScheduledAt = post.ScheduledAt ?? DateTime.UtcNow,
                Status = (int)PostScheduleStatus.Pending
            }, cancellationToken);

            post.Status = (int)PostStatus.Scheduled;
            await _posts.UpdateAsync(post, cancellationToken);
        }

        timeline.Status = (int)TimelineStatus.Active;
        await _timelines.UpdateAsync(timeline, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        response.ScheduledCount = remainingPosts.Count;
        return response;
    }
}
