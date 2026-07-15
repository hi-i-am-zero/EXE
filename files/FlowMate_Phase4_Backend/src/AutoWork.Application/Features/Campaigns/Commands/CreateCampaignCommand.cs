using AutoWork.Application.DTOs.Campaigns;
using MediatR;

namespace AutoWork.Application.Features.Campaigns.Commands;

public class CreateCampaignCommand : IRequest<CampaignDto>
{
    public CreateCampaignDto Request { get; set; } = null!;
}
