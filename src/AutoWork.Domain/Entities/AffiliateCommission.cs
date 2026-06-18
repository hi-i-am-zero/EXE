using AutoWork.Domain.Common;

namespace AutoWork.Domain.Entities;

/// <summary>Commission earned from a referred user conversion.</summary>
public class AffiliateCommission : BaseEntity
{
    public Guid AffiliateId { get; set; }

    public Guid ReferredUserId { get; set; }

    public Guid? SubscriptionId { get; set; }

    public decimal Amount { get; set; }

    public int Status { get; set; }

    public DateTime? PaidAt { get; set; }

    public Affiliate Affiliate { get; set; } = null!;

    public User ReferredUser { get; set; } = null!;

    public Subscription? Subscription { get; set; }
}
