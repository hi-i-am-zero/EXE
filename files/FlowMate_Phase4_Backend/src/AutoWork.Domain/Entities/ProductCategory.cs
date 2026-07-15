using AutoWork.Domain.Common;

namespace AutoWork.Domain.Entities;

/// <summary>Product category under a BrandProfile (Váy, Áo, Quần, Phụ kiện...).</summary>
public class ProductCategory : BaseEntity
{
    public Guid BrandProfileId { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public BrandProfile BrandProfile { get; set; } = null!;

    public ICollection<Product> Products { get; set; } = new List<Product>();
}
