using AutoWork.Domain.Common;

namespace AutoWork.Domain.Entities;

/// <summary>Join entity linking roles to permissions.</summary>
public class RolePermission : BaseEntity
{
    public Guid RoleId { get; set; }

    public Guid PermissionId { get; set; }

    public Role Role { get; set; } = null!;

    public Permission Permission { get; set; } = null!;
}
