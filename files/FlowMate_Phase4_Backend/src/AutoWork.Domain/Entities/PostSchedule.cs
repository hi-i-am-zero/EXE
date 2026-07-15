using AutoWork.Domain.Common;

namespace AutoWork.Domain.Entities;

/// <summary>Scheduled execution window for a post.</summary>
public class PostSchedule : BaseEntity
{
    public Guid PostId { get; set; }

    /// <summary>NULL = lịch chung cho cả bài viết (legacy). Nếu bài viết đăng nhiều nền tảng,
    /// nên gắn theo từng PostChannelAccount để retry độc lập theo từng kênh.</summary>
    public Guid? PostChannelAccountId { get; set; }

    public DateTime ScheduledAt { get; set; }

    public int Status { get; set; }

    public DateTime? ExecutedAt { get; set; }

    public string? FailureReason { get; set; }

    public int RetryCount { get; set; }

    public Post Post { get; set; } = null!;

    public PostChannelAccount? PostChannelAccount { get; set; }
}
