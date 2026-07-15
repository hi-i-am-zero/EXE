using AutoWork.Domain.Common;

namespace AutoWork.Domain.Entities;

/// <summary>Connected Facebook user account.</summary>
public class FacebookAccount : BaseEntity
{
    public Guid UserId { get; set; }

    /// <summary>Workspace mà liên kết Facebook này thuộc về — dùng để tự động tạo ChannelAccount
    /// tương ứng cho từng Fanpage, giúp Post/Campaign (theo ProjectId) chọn được nền tảng này.</summary>
    public Guid ProjectId { get; set; }

    public string FacebookUserId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? ProfilePictureUrl { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime? TokenExpiresAt { get; set; }

    public DateTime? LastSyncedAt { get; set; }

    public User User { get; set; } = null!;

    public ICollection<FacebookPage> Pages { get; set; } = new List<FacebookPage>();
}
