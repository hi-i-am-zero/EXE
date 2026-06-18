using AutoWork.Domain.Common;

namespace AutoWork.Domain.Entities;

/// <summary>AI-generated content result from a prompt.</summary>
public class AiGeneratedContent : BaseEntity
{
    public Guid AiPromptId { get; set; }

    public Guid UserId { get; set; }

    public Guid? ProjectId { get; set; }

    public string Input { get; set; } = string.Empty;

    public string? Output { get; set; }

    public int TokensUsed { get; set; }

    public int Status { get; set; }

    public string? ErrorMessage { get; set; }

    public AiPrompt AiPrompt { get; set; } = null!;

    public User User { get; set; } = null!;

    public Project? Project { get; set; }
}
