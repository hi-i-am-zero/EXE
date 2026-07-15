using AutoWork.Domain.Common;

namespace AutoWork.Domain.Entities;

/// <summary>
/// Bảng trung gian N-N CHÍNH: 1 Post có thể đăng lên NHIỀU ChannelAccount cùng lúc
/// (VD: Facebook Page A + Instagram B), MỖI ChannelAccount có Status/ExternalPostId/
/// PublishedUrl RIÊNG (tương tự "mỗi sản phẩm có số lượng riêng" trong 1 đơn hàng).
/// </summary>
public class PostChannelAccount : BaseEntity
{
    public Guid PostId { get; set; }

    public Guid ChannelAccountId { get; set; }

    /// <summary>enum AutoWork.Domain.Enums.PostStatus (Scheduled/Published/Failed riêng theo từng kênh).</summary>
    public int Status { get; set; }

    public string? ExternalPostId { get; set; }

    public string? PublishedUrl { get; set; }

    public DateTime? PublishedAt { get; set; }

    public Post Post { get; set; } = null!;

    public ChannelAccount ChannelAccount { get; set; } = null!;

    public PostAnalytics? Analytics { get; set; }

    public ICollection<PostSchedule> Schedules { get; set; } = new List<PostSchedule>();
}
