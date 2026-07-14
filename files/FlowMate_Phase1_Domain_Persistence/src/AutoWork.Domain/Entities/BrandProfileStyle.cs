using AutoWork.Domain.Common;

namespace AutoWork.Domain.Entities;

/// <summary>Join entity: BrandProfile &lt;-&gt; BrandStyle (business rule: tối đa 2, enforced ở Application layer).</summary>
public class BrandProfileStyle : BaseEntity
{
    public Guid BrandProfileId { get; set; }

    public Guid StyleId { get; set; }

    public BrandProfile BrandProfile { get; set; } = null!;

    public BrandStyle Style { get; set; } = null!;
}
