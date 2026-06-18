using AutoWork.Domain.Common;

namespace AutoWork.Domain.Entities;

/// <summary>Security role for authorization.</summary>
public class Role : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsSystem { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
