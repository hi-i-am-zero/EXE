namespace AutoWork.Application.DTOs.WordPress;

public class CreateWordPressPostDto
{
    public Guid WordPressSiteId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Status { get; set; } = "draft";
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? Slug { get; set; }
    public string? FocusKeyword { get; set; }
    public IReadOnlyList<string>? Categories { get; set; }
    public IReadOnlyList<string>? Tags { get; set; }
    public DateTime? ScheduledAt { get; set; }
}
