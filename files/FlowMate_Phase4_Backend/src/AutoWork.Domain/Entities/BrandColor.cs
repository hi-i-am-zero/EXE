using AutoWork.Domain.Common;

namespace AutoWork.Domain.Entities;

/// <summary>A brand color swatch (primary/secondary) attached to a BrandProfile.</summary>
public class BrandColor : BaseEntity
{
    public Guid BrandProfileId { get; set; }

    public string ColorHex { get; set; } = string.Empty;

    /// <summary>"Primary" or "Secondary".</summary>
    public string ColorType { get; set; } = "Secondary";

    public int SortOrder { get; set; }

    public BrandProfile BrandProfile { get; set; } = null!;
}
