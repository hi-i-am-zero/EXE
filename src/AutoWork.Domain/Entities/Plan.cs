using AutoWork.Domain.Common;

namespace AutoWork.Domain.Entities;

/// <summary>Subscription plan offering.</summary>
public class Plan : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string Code { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int BillingPeriod { get; set; }

    public int MaxProjects { get; set; }

    public int MaxChannels { get; set; }

    public int MaxPostsPerMonth { get; set; }

    public int CreditsIncluded { get; set; }

    public bool IsActive { get; set; } = true;

    public int SortOrder { get; set; }

    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}
