namespace AutoWork.Application.DTOs.AI;

public class AiHistoryDto
{
    public Guid Id { get; set; }
    public string Input { get; set; } = string.Empty;
    public string? Output { get; set; }
    public int TokensUsed { get; set; }
    public int Status { get; set; }
    public string? ErrorMessage { get; set; }
    public Guid? PromptId { get; set; }
    public string? PromptName { get; set; }
    public Guid? ProjectId { get; set; }
    public DateTime CreatedAt { get; set; }
}
