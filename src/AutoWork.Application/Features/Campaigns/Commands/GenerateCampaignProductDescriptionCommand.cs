using AutoWork.Application.DTOs.AI;
using MediatR;

namespace AutoWork.Application.Features.Campaigns.Commands;

public class GenerateCampaignProductDescriptionCommand : IRequest<GenerateCampaignProductDescriptionResponse>
{
    public GenerateCampaignProductDescriptionRequest Request { get; set; } = null!;
}
