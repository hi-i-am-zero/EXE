namespace AutoWork.Domain.Common;

/// <summary>
/// Base type for all domain entities with identity, audit fields, and soft-delete support.
/// </summary>
public abstract class BaseEntity : IAuditableEntity
{
    public Guid Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }
}
