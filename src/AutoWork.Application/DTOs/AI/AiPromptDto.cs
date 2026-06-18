namespace AutoWork.Application.DTOs.AI;

public class AiPromptDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Template { get; set; } = string.Empty;
    public string? Category { get; set; }
    public bool IsSystem { get; set; }
    public Guid? ProjectId { get; set; }
    public DateTime CreatedAt { get; set; }
}
