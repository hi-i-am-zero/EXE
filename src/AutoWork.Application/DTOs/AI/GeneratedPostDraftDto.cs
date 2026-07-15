namespace AutoWork.Application.DTOs.AI;

public class GeneratedPostDraftDto
{
    public Guid PostId { get; set; }
    public int DayNumber { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public List<string> Hashtags { get; set; } = [];
}
