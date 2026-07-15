using AutoWork.Domain.Common;

namespace AutoWork.Domain.Entities;

/// <summary>A brand font (title/body) attached to a BrandProfile.</summary>
public class BrandFont : BaseEntity
{
    public Guid BrandProfileId { get; set; }

    public string FontName { get; set; } = string.Empty;

    /// <summary>"Title" or "Body".</summary>
    public string UsageType { get; set; } = "Body";

    public BrandProfile BrandProfile { get; set; } = null!;
}
