namespace AutoWork.Application.DTOs.AI;

public class GenerateTimelinePostsResponse
{
    public Guid TimelineId { get; set; }
    public List<GeneratedPostDraftDto> Posts { get; set; } = [];
    public int CreditsUsed { get; set; }
    public int TokensUsed { get; set; }
}
