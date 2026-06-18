using AutoWork.Domain.Common;

namespace AutoWork.Domain.Entities;

/// <summary>Activity log entry for post publishing events.</summary>
public class PostLog : BaseEntity
{
    public Guid PostId { get; set; }

    public string Action { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public int Level { get; set; }

    public string? Details { get; set; }

    public Post Post { get; set; } = null!;
}
