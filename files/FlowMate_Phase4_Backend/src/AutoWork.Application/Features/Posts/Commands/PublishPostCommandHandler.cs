using AutoWork.Application.Common.Exceptions;
using AutoWork.Application.DTOs.Facebook;
using AutoWork.Application.DTOs.Posts;
using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Application.Interfaces.Services;
using AutoWork.Domain.Entities;
using AutoWork.Domain.Enums;
using MediatR;

namespace AutoWork.Application.Features.Posts.Commands;

public class PublishPostCommandHandler : IRequestHandler<PublishPostCommand, PublishPostResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<PostChannelAccount> _postChannelAccounts;
    private readonly IFacebookService _facebookService;
    private readonly ICurrentUserService _currentUser;

    public PublishPostCommandHandler(
        IUnitOfWork unitOfWork,
        IRepository<PostChannelAccount> postChannelAccounts,
        IFacebookService facebookService,
        ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _postChannelAccounts = postChannelAccounts;
        _facebookService = facebookService;
        _currentUser = currentUser;
    }

    public async Task<PublishPostResponse> Handle(PublishPostCommand command, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not Guid userId)
        {
            throw new UnauthorizedException("User is not authenticated.");
        }

        var post = await _unitOfWork.Posts.GetByIdWithDetailsAsync(command.PostId, cancellationToken)
            ?? throw new NotFoundException(nameof(Post), command.PostId);

        var project = await _unitOfWork.Projects.GetByIdAsync(post.ProjectId, cancellationToken);
        if (project is null || project.UserId != userId)
        {
            throw new UnauthorizedException("Bạn không có quyền đăng bài viết này.");
        }

        var content = post.Contents.OrderBy(c => c.SortOrder).FirstOrDefault()?.Content ?? post.Title;
        var results = new List<PublishPostResultDto>();

        // Legacy: nếu bài viết cũ chỉ có 1 ChannelAccountId đơn lẻ (chưa qua PostChannelAccounts),
        // vẫn coi đó là 1 kênh cần đăng để không "bỏ quên" các bài tạo trước khi có đa nền tảng.
        var targets = post.PostChannelAccounts.Count > 0
            ? post.PostChannelAccounts.ToList()
            : (post.ChannelAccount is not null
                ? [new PostChannelAccount { ChannelAccount = post.ChannelAccount, ChannelAccountId = post.ChannelAccount.Id, PostId = post.Id, Status = post.Status }]
                : []);

        foreach (var target in targets)
        {
            var result = new PublishPostResultDto
            {
                ChannelAccountId = target.ChannelAccountId,
                ChannelCode = target.ChannelAccount.Channel.Code,
                ChannelAccountName = target.ChannelAccount.Name
            };

            if (target.ChannelAccount.Channel.Code == "facebook")
            {
                await PublishToFacebookAsync(userId, post, target, result, cancellationToken);
            }
            else
            {
                result.Success = false;
                result.ErrorMessage = $"Chưa hỗ trợ đăng tự động qua API cho nền tảng '{target.ChannelAccount.Channel.Name}' — sẽ bổ sung ở giai đoạn sau.";
            }

            results.Add(result);
        }

        post.Status = results.Count > 0 && results.All(r => r.Success)
            ? (int)PostStatus.Published
            : results.Any(r => r.Success) ? (int)PostStatus.Published : (int)PostStatus.Failed;
        await _unitOfWork.Posts.UpdateAsync(post, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new PublishPostResponse { PostId = post.Id, PostStatus = post.Status, Results = results };
    }

    private async Task PublishToFacebookAsync(
        Guid userId, Post post, PostChannelAccount target, PublishPostResultDto result, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(target.ChannelAccount.ExternalId, out var facebookPageId))
        {
            result.Success = false;
            result.ErrorMessage = "Kênh Facebook này chưa được kết nối qua OAuth thật (thiếu liên kết Fanpage). Vui lòng kết nối lại ở trang Kênh bán hàng.";
            return;
        }

        try
        {
            var externalPostId = await _facebookService.PublishPostAsync(userId, new PublishFacebookPostDto
            {
                FacebookPageId = facebookPageId,
                Message = post.Contents.OrderBy(c => c.SortOrder).FirstOrDefault()?.Content ?? post.Title,
                ScheduledAt = post.ScheduledAt
            }, cancellationToken);

            result.Success = true;
            result.ExternalPostId = externalPostId;
            result.PublishedUrl = $"https://facebook.com/{externalPostId}";

            target.Status = (int)PostStatus.Published;
            target.ExternalPostId = externalPostId;
            target.PublishedUrl = result.PublishedUrl;
            target.PublishedAt = DateTime.UtcNow;

            if (target.Id != Guid.Empty) // bỏ qua nếu là bản ghi tạm dựng cho ChannelAccount đơn lẻ (legacy)
            {
                await _postChannelAccounts.UpdateAsync(target, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;

            target.Status = (int)PostStatus.Failed;
            if (target.Id != Guid.Empty)
            {
                await _postChannelAccounts.UpdateAsync(target, cancellationToken);
            }
        }
    }
}
