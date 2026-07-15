using AutoWork.Application.Common.Exceptions;
using AutoWork.Application.DTOs.AI;
using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Application.Interfaces.Services;
using AutoWork.Domain.Entities;
using MediatR;

namespace AutoWork.Application.Features.Timelines.Commands;

public class GenerateTimelinePostsCommandHandler : IRequestHandler<GenerateTimelinePostsCommand, GenerateTimelinePostsResponse>
{
    private readonly ITimelineContentService _timelineContentService;
    private readonly IRepository<Timeline> _timelines;
    private readonly IProjectAccessGuard _accessGuard;
    private readonly ICurrentUserService _currentUser;

    public GenerateTimelinePostsCommandHandler(
        ITimelineContentService timelineContentService,
        IRepository<Timeline> timelines,
        IProjectAccessGuard accessGuard,
        ICurrentUserService currentUser)
    {
        _timelineContentService = timelineContentService;
        _timelines = timelines;
        _accessGuard = accessGuard;
        _currentUser = currentUser;
    }

    public async Task<GenerateTimelinePostsResponse> Handle(GenerateTimelinePostsCommand command, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not Guid userId)
        {
            throw new UnauthorizedException("User is not authenticated.");
        }

        var timeline = await _timelines.GetByIdAsync(command.Request.TimelineId, cancellationToken)
            ?? throw new NotFoundException(nameof(Timeline), command.Request.TimelineId);

        await _accessGuard.EnsureEditorAsync(timeline.ProjectId, userId, cancellationToken);

        return await _timelineContentService.GenerateTimelinePostsAsync(userId, command.Request, cancellationToken);
    }
}
