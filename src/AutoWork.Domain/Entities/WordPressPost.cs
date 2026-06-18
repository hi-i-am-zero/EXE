using AutoWork.Domain.Common;

namespace AutoWork.Domain.Entities;

/// <summary>Post published or synced on a WordPress site.</summary>
public class WordPressPost : BaseEntity
{
    public Guid WordPressSiteId { get; set; }

    public string ExternalPostId { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string? Excerpt { get; set; }

    public int Status { get; set; }

    public string? Permalink { get; set; }

    public DateTime? PublishedAt { get; set; }

    public WordPressSite WordPressSite { get; set; } = null!;
}
