using AutoWork.Domain.Common;

namespace AutoWork.Domain.Entities;

/// <summary>JWT refresh token for user authentication.</summary>
public class RefreshToken : BaseEntity
{
    public Guid UserId { get; set; }

    public string Token { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public DateTime? RevokedAt { get; set; }

    public string? ReplacedByToken { get; set; }

    public string? CreatedByIp { get; set; }

    public bool IsRevoked { get; set; }

    public bool IsActive => !IsRevoked && RevokedAt is null && ExpiresAt > DateTime.UtcNow;

    public User User { get; set; } = null!;
}
