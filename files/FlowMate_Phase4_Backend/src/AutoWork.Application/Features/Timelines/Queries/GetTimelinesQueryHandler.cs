using AutoWork.Application.Common.Exceptions;
using AutoWork.Application.DTOs.Timelines;
using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Application.Interfaces.Services;
using AutoWork.Domain.Entities;
using MediatR;

namespace AutoWork.Application.Features.Timelines.Queries;

public class GetTimelinesQueryHandler : IRequestHandler<GetTimelinesQuery, List<TimelineDto>>
{
    private readonly IRepository<Timeline> _timelines;
    private readonly IRepository<TimelineTemplateType> _templateTypes;
    private readonly IRepository<Post> _posts;
    private readonly IProjectAccessGuard _accessGuard;
    private readonly ICurrentUserService _currentUser;

    public GetTimelinesQueryHandler(
        IRepository<Timeline> timelines,
        IRepository<TimelineTemplateType> templateTypes,
        IRepository<Post> posts,
        IProjectAccessGuard accessGuard,
        ICurrentUserService currentUser)
    {
        _timelines = timelines;
        _templateTypes = templateTypes;
        _posts = posts;
        _accessGuard = accessGuard;
        _currentUser = currentUser;
    }

    public async Task<List<TimelineDto>> Handle(GetTimelinesQuery request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not Guid userId)
        {
            throw new UnauthorizedException("User is not authenticated.");
        }

        await _accessGuard.EnsureViewerAsync(request.ProjectId, userId, cancellationToken);

        var timelines = await _timelines.FindAsync(t => t.ProjectId == request.ProjectId, cancellationToken);
        var templateTypeNames = (await _templateTypes.GetAllAsync(cancellationToken)).ToDictionary(t => t.Id, t => t.TypeName);

        // Đếm số bài viết của TẤT CẢ timeline trong 1 query duy nhất (group trong bộ nhớ),
        // thay vì gọi CountAsync riêng cho từng timeline (N+1).
        var timelineIds = timelines.Select(t => t.Id).ToList();
        var postCountByTimeline = (await _posts.FindAsync(p => p.TimelineId != null && timelineIds.Contains(p.TimelineId!.Value), cancellationToken))
            .GroupBy(p => p.TimelineId!.Value)
            .ToDictionary(g => g.Key, g => g.Count());

        var result = timelines
            .OrderByDescending(t => t.CreatedAt)
            .Select(timeline => new TimelineDto
            {
                Id = timeline.Id,
                ProjectId = timeline.ProjectId,
                CampaignId = timeline.CampaignId,
                Name = timeline.Name,
                TemplateTypeName = templateTypeNames.GetValueOrDefault(timeline.TemplateTypeId, string.Empty),
                StartDate = timeline.StartDate,
                EndDate = timeline.EndDate,
                Status = timeline.Status,
                PostCount = postCountByTimeline.GetValueOrDefault(timeline.Id, 0),
                CreatedAt = timeline.CreatedAt
            })
            .ToList();

        return result;
    }
}
