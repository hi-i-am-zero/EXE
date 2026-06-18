using AutoWork.Domain.Common;

namespace AutoWork.Domain.Entities;

/// <summary>Post published or synced on Zalo.</summary>
public class ZaloPost : BaseEntity
{
    public Guid ZaloAccountId { get; set; }

    public string? ExternalPostId { get; set; }

    public string Content { get; set; } = string.Empty;

    public int Status { get; set; }

    public DateTime? PublishedAt { get; set; }

    public ZaloAccount ZaloAccount { get; set; } = null!;
}
