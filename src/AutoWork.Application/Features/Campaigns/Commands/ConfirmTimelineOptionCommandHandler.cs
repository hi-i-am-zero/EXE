using AutoWork.Application.Common.Exceptions;
using AutoWork.Application.DTOs.Timelines;
using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Application.Interfaces.Services;
using AutoWork.Domain.Entities;
using MediatR;

namespace AutoWork.Application.Features.Campaigns.Commands;

/// <summary>User đã chọn 1 trong 3 phương án timeline — thật sự tạo Timeline + Post + PostContent + Hashtag.</summary>
public class ConfirmTimelineOptionCommandHandler : IRequestHandler<ConfirmTimelineOptionCommand, TimelineDto>
{
    private readonly IRepository<Campaign> _campaigns;
    private readonly ITimelineContentService _timelineContentService;
    private readonly IProjectAccessGuard _accessGuard;
    private readonly ICurrentUserService _currentUser;

    public ConfirmTimelineOptionCommandHandler(
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

    public async Task<TimelineDto> Handle(ConfirmTimelineOptionCommand command, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not Guid userId)
        {
            throw new UnauthorizedException("User is not authenticated.");
        }

        var campaign = await _campaigns.GetByIdAsync(command.Request.CampaignId, cancellationToken)
            ?? throw new NotFoundException(nameof(Campaign), command.Request.CampaignId);

        await _accessGuard.EnsureEditorAsync(campaign.ProjectId, userId, cancellationToken);

        return await _timelineContentService.ConfirmTimelineOptionAsync(userId, command.Request, cancellationToken);
    }
}
