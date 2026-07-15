using AutoWork.Shared.Enums;

namespace AutoWork.Application.DTOs.AI;

public class GenerateContentRequest
{
    public string Topic { get; set; } = string.Empty;
    public string? Keywords { get; set; }
    public int Length { get; set; } = 500;
    public string? Tone { get; set; }
    public string? Industry { get; set; }
    public string? Cta { get; set; }
    public AiProvider Provider { get; set; } = AiProvider.OpenAI;
    public Guid? PromptId { get; set; }
    public Guid? ProjectId { get; set; }
}
