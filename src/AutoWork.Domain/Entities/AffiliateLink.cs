using AutoWork.Domain.Common;

namespace AutoWork.Domain.Entities;

/// <summary>Trackable affiliate referral link.</summary>
public class AffiliateLink : BaseEntity
{
    public Guid AffiliateId { get; set; }

    public string Url { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Campaign { get; set; }

    public int ClickCount { get; set; }

    public int ConversionCount { get; set; }

    public bool IsActive { get; set; } = true;

    public Affiliate Affiliate { get; set; } = null!;
}
