using AutoWork.Domain.Common;

namespace AutoWork.Domain.Entities;

/// <summary>Join entity linking users to roles.</summary>
public class UserRole : BaseEntity
{
    public Guid UserId { get; set; }

    public Guid RoleId { get; set; }

    public User User { get; set; } = null!;

    public Role Role { get; set; } = null!;
}
