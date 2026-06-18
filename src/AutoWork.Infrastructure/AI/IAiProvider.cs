using AutoWork.Shared.Enums;

namespace AutoWork.Infrastructure.AI;

public interface IAiProvider
{
    AiProvider Provider { get; }

    Task<AiGenerationResult> GenerateAsync(AiGenerationRequest request, CancellationToken cancellationToken = default);
}

public class AiGenerationRequest
{
    public string SystemPrompt { get; set; } = string.Empty;

    public string UserPrompt { get; set; } = string.Empty;

    public int MaxTokens { get; set; } = 2048;
}

public class AiGenerationResult
{
    public string RawContent { get; set; } = string.Empty;

    public int TokensUsed { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string Content { get; set; } = string.Empty;

    public string? Hashtags { get; set; }
}
