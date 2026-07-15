using AutoWork.Domain.Common;

namespace AutoWork.Domain.Entities;

/// <summary>Lookup: tone-of-voice keywords (Gần gũi, Dí dỏm, Trẻ trung...).</summary>
public class ToneKeyword : BaseEntity
{
    public string Keyword { get; set; } = string.Empty;

    public ICollection<BrandProfileKeyword> BrandProfileKeywords { get; set; } = new List<BrandProfileKeyword>();
}
