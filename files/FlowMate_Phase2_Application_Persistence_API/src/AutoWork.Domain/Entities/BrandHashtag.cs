using AutoWork.Domain.Common;

namespace AutoWork.Domain.Entities;

/// <summary>Join entity: BrandProfile &lt;-&gt; Hashtag.</summary>
public class BrandHashtag : BaseEntity
{
    public Guid BrandProfileId { get; set; }

    public Guid HashtagId { get; set; }

    public BrandProfile BrandProfile { get; set; } = null!;

    public Hashtag Hashtag { get; set; } = null!;
}
