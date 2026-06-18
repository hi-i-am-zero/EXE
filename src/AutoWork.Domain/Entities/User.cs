using AutoWork.Domain.Common;

namespace AutoWork.Domain.Entities;

/// <summary>Application user account.</summary>
public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string? AvatarUrl { get; set; }

    public string? Phone { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime? LastLoginAt { get; set; }

    public string? GoogleId { get; set; }

    public string? FacebookId { get; set; }

    public string? ReferralCode { get; set; }

    public Guid? ReferredByUserId { get; set; }

    public User? ReferredByUser { get; set; }

    public ICollection<User> ReferredUsers { get; set; } = new List<User>();

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();

    public Credit? Credit { get; set; }

    public ICollection<CreditTransaction> CreditTransactions { get; set; } = new List<CreditTransaction>();

    public ICollection<Project> Projects { get; set; } = new List<Project>();

    public ICollection<ChannelAccount> ChannelAccounts { get; set; } = new List<ChannelAccount>();

    public ICollection<FacebookAccount> FacebookAccounts { get; set; } = new List<FacebookAccount>();

    public ICollection<WordPressSite> WordPressSites { get; set; } = new List<WordPressSite>();

    public ICollection<ZaloAccount> ZaloAccounts { get; set; } = new List<ZaloAccount>();

    public ICollection<MediaFile> MediaFiles { get; set; } = new List<MediaFile>();

    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public Affiliate? Affiliate { get; set; }

    public ICollection<AffiliateCommission> AffiliateCommissions { get; set; } = new List<AffiliateCommission>();

    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    public ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public ICollection<AiPrompt> AiPrompts { get; set; } = new List<AiPrompt>();

    public ICollection<AiGeneratedContent> AiGeneratedContents { get; set; } = new List<AiGeneratedContent>();

    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    public ICollection<PasswordResetToken> PasswordResetTokens { get; set; } = new List<PasswordResetToken>();

    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    public string FullName => $"{FirstName} {LastName}".Trim();
}
