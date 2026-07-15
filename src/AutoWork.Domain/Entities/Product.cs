using AutoWork.Domain.Common;

namespace AutoWork.Domain.Entities;

/// <summary>A product belonging to a brand, used for AI-generated descriptions.</summary>
public class Product : BaseEntity
{
    public Guid BrandProfileId { get; set; }

    public Guid? ProductCategoryId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public decimal? Price { get; set; }

    public string? AiGeneratedDescription { get; set; }

    public BrandProfile BrandProfile { get; set; } = null!;

    public ProductCategory? ProductCategory { get; set; }

    public ICollection<MediaFile> Images { get; set; } = new List<MediaFile>();
}
