namespace AutoWork.Application.DTOs.Zalo;

public class ZaloPostDto
{
    public Guid Id { get; set; }
    public Guid ZaloAccountId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? ExternalPostId { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public string? MediaUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}
