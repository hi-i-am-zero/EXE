using AutoWork.Domain.Common;

namespace AutoWork.Domain.Entities;

/// <summary>Scheduled execution window for a post.</summary>
public class PostSchedule : BaseEntity
{
    public Guid PostId { get; set; }

    public DateTime ScheduledAt { get; set; }

    public int Status { get; set; }

    public DateTime? ExecutedAt { get; set; }

    public string? FailureReason { get; set; }

    public int RetryCount { get; set; }

    public Post Post { get; set; } = null!;
}
