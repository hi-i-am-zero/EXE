namespace AutoWork.Application.DTOs.Zalo;

public class PublishZaloPostRequest
{
    public Guid ZaloAccountId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? MediaUrl { get; set; }
    public DateTime? ScheduledAt { get; set; }
}
