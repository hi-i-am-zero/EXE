namespace AutoWork.Application.DTOs.Workspace;

public class AddProjectMemberDto
{
    public Guid ProjectId { get; set; }

    public string Email { get; set; } = string.Empty;

    /// <summary>Owner / Admin / Member.</summary>
    public string Role { get; set; } = "Member";
}
