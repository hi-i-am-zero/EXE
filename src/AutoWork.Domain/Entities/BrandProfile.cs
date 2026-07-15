using AutoWork.Domain.Common;

namespace AutoWork.Domain.Entities;

/// <summary>Brand Memory: mô tả thương hiệu của 1 Project, nằm trong Hồ sơ tài khoản.
/// Quan hệ 1-1 với Project (mỗi workspace/shop có đúng 1 hồ sơ thương hiệu).</summary>
public class BrandProfile : BaseEntity
{
    public Guid ProjectId { get; set; }

    public string? LogoUrl { get; set; }

    public string BusinessName { get; set; } = string.Empty;

    public string BrandName { get; set; } = string.Empty;

    public string? Industry { get; set; }

    public short? FoundingYear { get; set; }

    public string? ShortDescription { get; set; }

    public string? Address { get; set; }

    public string? ContactPhone { get; set; }

    public string? ContactEmail { get; set; }

    public string? Website { get; set; }

    public Guid? VoiceSampleId { get; set; }

    public Project Project { get; set; } = null!;

    public VoiceSampleTemplate? VoiceSample { get; set; }

    public ICollection<BrandProfileStyle> BrandProfileStyles { get; set; } = new List<BrandProfileStyle>();

    public ICollection<BrandProfileKeyword> BrandProfileKeywords { get; set; } = new List<BrandProfileKeyword>();

    public ICollection<BrandColor> Colors { get; set; } = new List<BrandColor>();

    public ICollection<BrandFont> Fonts { get; set; } = new List<BrandFont>();

    public ICollection<BrandCta> BrandCtas { get; set; } = new List<BrandCta>();

    public ICollection<BrandHashtag> BrandHashtags { get; set; } = new List<BrandHashtag>();

    public ICollection<ProductCategory> ProductCategories { get; set; } = new List<ProductCategory>();

    public ICollection<Product> Products { get; set; } = new List<Product>();
}
