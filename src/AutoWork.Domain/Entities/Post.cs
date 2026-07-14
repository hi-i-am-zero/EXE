using AutoWork.Domain.Common;

namespace AutoWork.Domain.Entities;

/// <summary>Social or content post scheduled for publishing.</summary>
public class Post : BaseEntity
{
    public Guid ProjectId { get; set; }

    /// <summary>Legacy channel link (compat với bản AutoWork cũ / EF). Nullable trên FlowMate schema v2.</summary>
    public Guid? ChannelAccountId { get; set; }

    public Guid? TimelineId { get; set; }

    public string Title { get; set; } = string.Empty;

    public int Status { get; set; }

    public DateTime? ScheduledAt { get; set; }

    public string? ExternalPostId { get; set; }

    public string? PublishedUrl { get; set; }

    public DateTime? PublishedAt { get; set; }

    public Project Project { get; set; } = null!;

    public ChannelAccount? ChannelAccount { get; set; }

    public ICollection<PostContent> Contents { get; set; } = new List<PostContent>();

    public PostSchedule? Schedule { get; set; }

    public ICollection<PostLog> Logs { get; set; } = new List<PostLog>();
}
