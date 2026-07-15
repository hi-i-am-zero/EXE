using AutoWork.Application.DTOs.Campaigns;
using MediatR;

namespace AutoWork.Application.Features.Campaigns.Queries;

public class GetCampaignsQuery : IRequest<List<CampaignDto>>
{
    public Guid ProjectId { get; set; }
}
