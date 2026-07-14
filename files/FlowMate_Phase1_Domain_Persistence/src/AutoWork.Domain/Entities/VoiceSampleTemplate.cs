using AutoWork.Domain.Common;

namespace AutoWork.Domain.Entities;

/// <summary>Sample sentence used to pick a brand's tone of voice ("Giọng văn").</summary>
public class VoiceSampleTemplate : BaseEntity
{
    public string SampleText { get; set; } = string.Empty;

    public ICollection<BrandProfile> BrandProfiles { get; set; } = new List<BrandProfile>();

    public ICollection<Post> Posts { get; set; } = new List<Post>();
}
