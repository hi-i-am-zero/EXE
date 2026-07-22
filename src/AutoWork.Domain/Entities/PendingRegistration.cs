using AutoWork.Domain.Common;

namespace AutoWork.Domain.Entities;

/// <summary>Đăng ký chờ xác nhận email — chưa tạo User cho đến khi verify.</summary>
public class PendingRegistration : BaseEntity
{
    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string ReferralCode { get; set; } = string.Empty;

    public Guid? ReferredByUserId { get; set; }

    public string Token { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public DateTime? UsedAt { get; set; }
}
