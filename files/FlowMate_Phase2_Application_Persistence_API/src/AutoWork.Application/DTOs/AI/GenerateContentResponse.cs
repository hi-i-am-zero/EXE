namespace AutoWork.Application.DTOs.AI;

public class GenerateContentResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? Hashtags { get; set; }
    public int CreditsUsed { get; set; }
    public int TokensUsed { get; set; }
}
