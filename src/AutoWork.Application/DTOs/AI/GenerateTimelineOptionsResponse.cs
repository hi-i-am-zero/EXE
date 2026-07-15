namespace AutoWork.Application.DTOs.AI;

public class GenerateTimelineOptionsResponse
{
    public Guid CampaignId { get; set; }
    public List<TimelineOptionDto> Options { get; set; } = [];
    public int CreditsUsed { get; set; }
    public int TokensUsed { get; set; }
}
