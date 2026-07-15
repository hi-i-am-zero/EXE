namespace AutoWork.Application.DTOs.Workspace;

public class UpdateProjectMemberRoleDto
{
    public Guid ProjectId { get; set; }

    public Guid UserId { get; set; }

    public string Role { get; set; } = "Member";
}
