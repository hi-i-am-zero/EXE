using AutoWork.Domain.Common;

namespace AutoWork.Domain.Entities;

/// <summary>Content block belonging to a post.</summary>
public class PostContent : BaseEntity
{
    public Guid PostId { get; set; }

    public int ContentType { get; set; }

    public string Content { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public Guid? MediaFileId { get; set; }

    public Post Post { get; set; } = null!;

    public MediaFile? MediaFile { get; set; }
}
