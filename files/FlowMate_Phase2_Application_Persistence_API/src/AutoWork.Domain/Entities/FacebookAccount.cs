using AutoWork.Domain.Common;

namespace AutoWork.Domain.Entities;

/// <summary>Connected Facebook user account.</summary>
public class FacebookAccount : BaseEntity
{
    public Guid UserId { get; set; }

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
