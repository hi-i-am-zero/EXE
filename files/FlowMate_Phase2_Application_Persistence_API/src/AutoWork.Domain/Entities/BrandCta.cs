using AutoWork.Domain.Common;

namespace AutoWork.Domain.Entities;

/// <summary>Join entity: BrandProfile &lt;-&gt; CtaTemplate.</summary>
public class BrandCta : BaseEntity
{
    public Guid BrandProfileId { get; set; }

    public Guid CtaId { get; set; }

    public BrandProfile BrandProfile { get; set; } = null!;

    public CtaTemplate Cta { get; set; } = null!;
}
