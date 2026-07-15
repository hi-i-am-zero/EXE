using AutoWork.Domain.Common;

namespace AutoWork.Domain.Entities;

/// <summary>Membership of a user inside a project/workspace (Owner/Admin/Member).</summary>
public class ProjectMember : BaseEntity
{
    public Guid ProjectId { get; set; }

    public Guid UserId { get; set; }

    public string Role { get; set; } = "Member";

    public DateTime? InvitedAt { get; set; }

    public DateTime? JoinedAt { get; set; }

    public Project Project { get; set; } = null!;

    public User User { get; set; } = null!;
}
