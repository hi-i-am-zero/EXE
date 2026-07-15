namespace AutoWork.Domain.Common;

/// <summary>
/// Marks an entity as auditable with creation and modification metadata.
/// </summary>
public interface IAuditableEntity
{
    DateTime CreatedAt { get; set; }

    DateTime? UpdatedAt { get; set; }

    Guid? CreatedBy { get; set; }

    Guid? UpdatedBy { get; set; }
}
