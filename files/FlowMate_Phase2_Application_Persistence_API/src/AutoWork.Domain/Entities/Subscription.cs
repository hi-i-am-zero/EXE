using AutoWork.Domain.Common;

namespace AutoWork.Domain.Entities;

/// <summary>User subscription to a plan.</summary>
public class Subscription : BaseEntity
{
    public Guid UserId { get; set; }

    public Guid PlanId { get; set; }

    public int Status { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public DateTime? CancelledAt { get; set; }

    public bool AutoRenew { get; set; } = true;

    public User User { get; set; } = null!;

    public Plan Plan { get; set; } = null!;

    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    public ICollection<AffiliateCommission> AffiliateCommissions { get; set; } = new List<AffiliateCommission>();
}
