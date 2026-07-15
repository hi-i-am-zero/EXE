using AutoWork.Application.Common.Exceptions;
using AutoWork.Application.DTOs.AI;
using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Application.Interfaces.Services;
using AutoWork.Domain.Entities;
using MediatR;

namespace AutoWork.Application.Features.Campaigns.Commands;

public class GenerateTimelineOptionsCommandHandler : IRequestHandler<GenerateTimelineOptionsCommand, GenerateTimelineOptionsResponse>
{
    private readonly IRepository<Campaign> _campaigns;
    private readonly ITimelineContentService _timelineContentService;
    private readonly IProjectAccessGuard _accessGuard;
    private readonly ICurrentUserService _currentUser;

    public GenerateTimelineOptionsCommandHandler(
        IRepository<Campaign> campaigns,
        ITimelineContentService timelineContentService,
        IProjectAccessGuard accessGuard,
        ICurrentUserService currentUser)
    {
        _campaigns = campaigns;
        _timelineContentService = timelineContentService;
        _accessGuard = accessGuard;
        _currentUser = currentUser;
    }

    public async Task<GenerateTimelineOptionsResponse> Handle(GenerateTimelineOptionsCommand command, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not Guid userId)
        {
            throw new UnauthorizedException("User is not authenticated.");
        }

        var campaign = await _campaigns.GetByIdAsync(command.Request.CampaignId, cancellationToken)
            ?? throw new NotFoundException(nameof(Campaign), command.Request.CampaignId);

        await _accessGuard.EnsureEditorAsync(campaign.ProjectId, userId, cancellationToken);

        return await _timelineContentService.GenerateTimelineOptionsAsync(userId, command.Request, cancellationToken);
    }
}
