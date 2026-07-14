using AutoWork.Domain.Common;

namespace AutoWork.Domain.Entities;

/// <summary>Số liệu hiệu suất theo từng cặp Post-ChannelAccount (1-1 với PostChannelAccount).</summary>
public class PostAnalytics : BaseEntity
{
    public Guid PostChannelAccountId { get; set; }

    public int Reach { get; set; }

    public int Likes { get; set; }

    public int Comments { get; set; }

    public int Shares { get; set; }

    public DateTime RecordedAt { get; set; }

    public PostChannelAccount PostChannelAccount { get; set; } = null!;
}
