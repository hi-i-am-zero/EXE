using AutoWork.Domain.Common;

namespace AutoWork.Domain.Entities;

/// <summary>Lookup: hashtag dùng chung cho Brand Memory và Post.</summary>
public class Hashtag : BaseEntity
{
    public string Tag { get; set; } = string.Empty;

    public ICollection<BrandHashtag> BrandHashtags { get; set; } = new List<BrandHashtag>();

    public ICollection<PostHashtag> PostHashtags { get; set; } = new List<PostHashtag>();
}
