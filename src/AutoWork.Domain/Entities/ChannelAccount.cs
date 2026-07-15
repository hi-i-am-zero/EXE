using AutoWork.Domain.Common;

namespace AutoWork.Domain.Entities;

/// <summary>Connected channel account within a project.</summary>
public class ChannelAccount : BaseEntity
{
    public Guid ProjectId { get; set; }

    public Guid ChannelId { get; set; }

    public Guid UserId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? ExternalId { get; set; }

    public string? ProfileUrl { get; set; }

    public string? AvatarUrl { get; set; }

    /// <summary>Hỗ trợ phân cấp — VD: 1 tài khoản Facebook cha quản lý nhiều Fanpage con.
    /// Mỗi Fanpage là 1 ChannelAccount con trỏ về tài khoản cha qua field này.</summary>
    public Guid? ParentChannelAccountId { get; set; }

    /// <summary>OAuth access token để gọi API đăng bài (mã hoá ở tầng Infrastructure trước khi lưu).
    /// Để trống ở giai đoạn hiện tại — sẽ điền khi làm tính năng liên kết Facebook/Zalo API (giai đoạn 2).</summary>
    public string? AccessToken { get; set; }

    public string? RefreshToken { get; set; }

    public DateTime? TokenExpiresAt { get; set; }

    public string? Scope { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime? LastSyncedAt { get; set; }

    public Project Project { get; set; } = null!;

    public Channel Channel { get; set; } = null!;

    public User User { get; set; } = null!;

    public ChannelAccount? ParentChannelAccount { get; set; }

    public ICollection<ChannelAccount> ChildChannelAccounts { get; set; } = new List<ChannelAccount>();

    public ICollection<Post> Posts { get; set; } = new List<Post>();

    public ICollection<PostChannelAccount> PostChannelAccounts { get; set; } = new List<PostChannelAccount>();

    public ICollection<CampaignChannelAccount> CampaignChannelAccounts { get; set; } = new List<CampaignChannelAccount>();
}
