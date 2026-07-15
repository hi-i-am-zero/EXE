using AutoWork.Domain.Common;

namespace AutoWork.Domain.Entities;

/// <summary>Lookup: brand style tags (Streetwear, Y2K, Basic...).</summary>
public class BrandStyle : BaseEntity
{
    public string StyleName { get; set; } = string.Empty;

    public ICollection<BrandProfileStyle> BrandProfileStyles { get; set; } = new List<BrandProfileStyle>();
}
