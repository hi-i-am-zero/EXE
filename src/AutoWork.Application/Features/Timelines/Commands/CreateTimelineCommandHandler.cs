using AutoWork.Application.Common.Exceptions;
using AutoWork.Application.DTOs.Timelines;
using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Application.Interfaces.Services;
using AutoWork.Domain.Entities;
using MediatR;

namespace AutoWork.Application.Features.Timelines.Commands;

public class CreateTimelineCommandHandler : IRequestHandler<CreateTimelineCommand, TimelineDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<Timeline> _timelines;
    private readonly IRepository<TimelineTemplateType> _templateTypes;
    private readonly IRepository<Campaign> _campaigns;
    private readonly IProjectAccessGuard _accessGuard;
    private readonly ICurrentUserService _currentUser;

    public CreateTimelineCommandHandler(
        IUnitOfWork unitOfWork,
        IRepository<Timeline> timelines,
        IRepository<TimelineTemplateType> templateTypes,
        IRepository<Campaign> campaigns,
        IProjectAccessGuard accessGuard,
        ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _timelines = timelines;
        _templateTypes = templateTypes;
        _campaigns = campaigns;
        _accessGuard = accessGuard;
        _currentUser = currentUser;
    }

    public async Task<TimelineDto> Handle(CreateTimelineCommand command, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not Guid userId)
        {
            throw new UnauthorizedException("User is not authenticated.");
        }

        await _accessGuard.EnsureEditorAsync(command.Request.ProjectId, userId, cancellationToken);

        var templateType = await _templateTypes.GetByIdAsync(command.Request.TemplateTypeId, cancellationToken)
            ?? throw new BadRequestException("Mẫu timeline không hợp lệ.");

        if (command.Request.CampaignId.HasValue)
        {
            var campaign = await _campaigns.GetByIdAsync(command.Request.CampaignId.Value, cancellationToken);
            if (campaign is null || campaign.ProjectId != command.Request.ProjectId)
            {
                throw new BadRequestException("Chiến dịch không thuộc workspace này.");
            }
        }

        var timeline = new Timeline
        {
            ProjectId = command.Request.ProjectId,
            CampaignId = command.Request.CampaignId,
            Name = command.Request.Name.Trim(),
            TemplateTypeId = command.Request.TemplateTypeId,
            StartDate = command.Request.StartDate,
            EndDate = command.Request.EndDate,
            Status = 0 // TimelineStatus.Draft
        };

        await _timelines.AddAsync(timeline, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new TimelineDto
        {
            Id = timeline.Id,
            ProjectId = timeline.ProjectId,
            CampaignId = timeline.CampaignId,
            Name = timeline.Name,
            TemplateTypeName = templateType.TypeName,
            StartDate = timeline.StartDate,
            EndDate = timeline.EndDate,
            Status = timeline.Status,
            PostCount = 0,
            CreatedAt = timeline.CreatedAt
        };
    }
}
