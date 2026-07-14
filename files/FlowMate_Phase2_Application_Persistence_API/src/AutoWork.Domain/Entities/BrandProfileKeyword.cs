using AutoWork.Domain.Common;

namespace AutoWork.Domain.Entities;

/// <summary>Join entity: BrandProfile &lt;-&gt; ToneKeyword (khuyến nghị 3-5 từ, enforced ở Application layer).</summary>
public class BrandProfileKeyword : BaseEntity
{
    public Guid BrandProfileId { get; set; }

    public Guid ToneKeywordId { get; set; }

    public BrandProfile BrandProfile { get; set; } = null!;

    public ToneKeyword ToneKeyword { get; set; } = null!;
}
