using AutoWork.Domain.Common;

namespace AutoWork.Domain.Entities;

/// <summary>Affiliate program participant profile.</summary>
public class Affiliate : BaseEntity
{
    public Guid UserId { get; set; }

    public string Code { get; set; } = string.Empty;

    public decimal CommissionRate { get; set; }

    public decimal TotalEarnings { get; set; }

    public bool IsActive { get; set; } = true;

    public User User { get; set; } = null!;

    public ICollection<AffiliateLink> Links { get; set; } = new List<AffiliateLink>();

    public ICollection<AffiliateCommission> Commissions { get; set; } = new List<AffiliateCommission>();
}
