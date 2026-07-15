using System.Text.Json;
using AutoWork.Shared.Enums;

namespace AutoWork.Infrastructure.AI;

public class AiProviderFactory
{
    private readonly IReadOnlyDictionary<AiProvider, IAiProvider> _providers;

    public AiProviderFactory(IEnumerable<IAiProvider> providers)
    {
        _providers = providers.ToDictionary(p => p.Provider);
    }

    public IAiProvider GetProvider(AiProvider provider)
    {
        if (!_providers.TryGetValue(provider, out var instance))
        {
            throw new NotSupportedException($"AI provider '{provider}' is not registered.");
        }

        return instance;
    }
}

public static class AiResponseParser
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static AiGenerationResult ParseStructuredResponse(string rawContent, int tokensUsed)
    {
        var result = new AiGenerationResult
        {
            RawContent = rawContent,
            TokensUsed = tokensUsed,
            Content = rawContent
        };

        try
        {
            var jsonStart = rawContent.IndexOf('{');
            var jsonEnd = rawContent.LastIndexOf('}');
            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                var json = rawContent[jsonStart..(jsonEnd + 1)];
                var parsed = JsonSerializer.Deserialize<AiStructuredContent>(json, JsonOptions);
                if (parsed is not null)
                {
                    result.Title = parsed.Title ?? string.Empty;
                    result.Description = parsed.Description;
                    result.Content = parsed.Content ?? rawContent;
                    result.Hashtags = parsed.Hashtags;
                }
            }
        }
        catch
        {
            result.Title = ExtractFirstLine(rawContent);
            result.Content = rawContent;
        }

        if (string.IsNullOrWhiteSpace(result.Title))
        {
            result.Title = ExtractFirstLine(result.Content);
        }

        return result;
    }

    private static string ExtractFirstLine(string content) =>
        content.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .FirstOrDefault() ?? "Generated Content";

    private sealed class AiStructuredContent
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Content { get; set; }
        public string? Hashtags { get; set; }
    }
}
