using AutoWork.Domain.Common;

namespace AutoWork.Domain.Entities;

/// <summary>One-time token for password reset flow.</summary>
public class PasswordResetToken : BaseEntity
{
    public Guid UserId { get; set; }

    public string Token { get; set; } = string.Empty;

    public string OtpCode { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public DateTime? UsedAt { get; set; }

    public User User { get; set; } = null!;
}
