namespace AutoWork.Application.DTOs.WordPress;

public class WordPressPostDto
{
    public Guid Id { get; set; }
    public Guid WordPressSiteId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? ExternalPostId { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? Slug { get; set; }
    public string? FocusKeyword { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
