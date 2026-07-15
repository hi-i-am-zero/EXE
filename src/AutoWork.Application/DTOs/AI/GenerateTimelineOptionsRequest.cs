using AutoWork.Shared.Enums;

namespace AutoWork.Application.DTOs.AI;

public class GenerateTimelineOptionsRequest
{
    public Guid CampaignId { get; set; }
    public AiProvider Provider { get; set; } = AiProvider.Claude;
}
