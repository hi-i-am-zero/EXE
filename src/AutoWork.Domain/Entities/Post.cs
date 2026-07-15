using AutoWork.Domain.Common;

namespace AutoWork.Domain.Entities;

/// <summary>Social or content post scheduled for publishing.</summary>
public class Post : BaseEntity
{
    public Guid ProjectId { get; set; }

    /// <summary>Legacy channel link (compat với bản AutoWork cũ / EF). Nullable trên FlowMate schema v2.</summary>
    public Guid? ChannelAccountId { get; set; }

    public Guid? TimelineId { get; set; }

    public Guid? VoiceSampleId { get; set; }

    public string Title { get; set; } = string.Empty;

    public int Status { get; set; }

    /// <summary>Vị trí Day 1/2/3 khi Post thuộc 1 Timeline dạng Classic Campaign.</summary>
    public int? DayNumber { get; set; }

    public DateTime? ScheduledAt { get; set; }

    public bool TrackingEnabled { get; set; } = true;

    /// <summary>Legacy — giữ lại để tương thích ChannelAccountId đơn lẻ cũ. Bài viết mới nên
    /// dùng PostChannelAccounts để hỗ trợ đăng lên nhiều nền tảng cùng lúc.</summary>
    public string? ExternalPostId { get; set; }

    public string? PublishedUrl { get; set; }

    public DateTime? PublishedAt { get; set; }

    public Project Project { get; set; } = null!;

    public ChannelAccount? ChannelAccount { get; set; }

    public Timeline? Timeline { get; set; }

    public VoiceSampleTemplate? VoiceSample { get; set; }

    public ICollection<PostContent> Contents { get; set; } = new List<PostContent>();

    public PostSchedule? Schedule { get; set; }

    public ICollection<PostLog> Logs { get; set; } = new List<PostLog>();

    /// <summary>N-N: các nền tảng cụ thể mà bài viết này được đăng lên, mỗi nền tảng có
    /// trạng thái/kết quả đăng riêng.</summary>
    public ICollection<PostChannelAccount> PostChannelAccounts { get; set; } = new List<PostChannelAccount>();

    public ICollection<PostHashtag> PostHashtags { get; set; } = new List<PostHashtag>();
}
