using AutoWork.Domain.Common;

namespace AutoWork.Domain.Entities;

/// <summary>Uploaded media asset.</summary>
public class MediaFile : BaseEntity
{
    public Guid UserId { get; set; }

    public Guid? ProjectId { get; set; }

    /// <summary>Gắn ảnh này với 1 sản phẩm cụ thể trong Brand Memory (nullable — không phải media nào cũng là ảnh sản phẩm).</summary>
    public Guid? ProductId { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string FileUrl { get; set; } = string.Empty;

    public string MimeType { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public string? ThumbnailUrl { get; set; }

    public int? Width { get; set; }

    public int? Height { get; set; }

    public User User { get; set; } = null!;

    public Project? Project { get; set; }

    public Product? Product { get; set; }

    public ICollection<PostContent> PostContents { get; set; } = new List<PostContent>();
}
