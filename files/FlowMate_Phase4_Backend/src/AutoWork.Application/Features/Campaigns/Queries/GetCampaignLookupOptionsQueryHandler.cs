using AutoWork.Application.DTOs.BrandMemory;
using AutoWork.Application.DTOs.Campaigns;
using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Domain.Entities;
using MediatR;

namespace AutoWork.Application.Features.Campaigns.Queries;

public class GetCampaignLookupOptionsQueryHandler : IRequestHandler<GetCampaignLookupOptionsQuery, CampaignLookupOptionsDto>
{
    private readonly IRepository<CampaignGoal> _goals;
    private readonly IRepository<PromotionType> _promoTypes;
    private readonly IRepository<TimelineTemplateType> _templateTypes;

    public GetCampaignLookupOptionsQueryHandler(
        IRepository<CampaignGoal> goals,
        IRepository<PromotionType> promoTypes,
        IRepository<TimelineTemplateType> templateTypes)
    {
        _goals = goals;
        _promoTypes = promoTypes;
        _templateTypes = templateTypes;
    }

    public async Task<CampaignLookupOptionsDto> Handle(GetCampaignLookupOptionsQuery request, CancellationToken cancellationToken)
    {
        var goals = await _goals.GetAllAsync(cancellationToken);
        var promoTypes = await _promoTypes.GetAllAsync(cancellationToken);
        var templateTypes = await _templateTypes.GetAllAsync(cancellationToken);

        return new CampaignLookupOptionsDto
        {
            Goals = goals.Select(g => new LookupItemDto { Id = g.Id, Name = g.GoalName }).ToList(),
            PromotionTypes = promoTypes.Select(t => new LookupItemDto { Id = t.Id, Name = t.TypeName }).ToList(),
            TimelineTemplateTypes = templateTypes.Select(t => new LookupItemDto { Id = t.Id, Name = t.TypeName }).ToList()
        };
    }
}
