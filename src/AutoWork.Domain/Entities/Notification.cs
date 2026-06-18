using AutoWork.Domain.Common;

namespace AutoWork.Domain.Entities;

/// <summary>In-app user notification.</summary>
public class Notification : BaseEntity
{
    public Guid UserId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public int Type { get; set; }

    public bool IsRead { get; set; }

    public DateTime? ReadAt { get; set; }

    public string? ReferenceType { get; set; }

    public Guid? ReferenceId { get; set; }

    public User User { get; set; } = null!;
}
