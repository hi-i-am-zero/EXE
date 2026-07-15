using AutoWork.Domain.Common;

namespace AutoWork.Domain.Entities;

/// <summary>Connected Zalo official account.</summary>
public class ZaloAccount : BaseEntity
{
    public Guid UserId { get; set; }

    public string ZaloUserId { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string? AvatarUrl { get; set; }

    public bool IsConnected { get; set; } = true;

    public DateTime? TokenExpiresAt { get; set; }

    public DateTime? LastSyncedAt { get; set; }

    public User User { get; set; } = null!;

    public ICollection<ZaloPost> Posts { get; set; } = new List<ZaloPost>();
}
