namespace AutoWork.Infrastructure.Settings;

public class AiSettings
{
    public const string SectionName = "AiSettings";

    public string DefaultModel { get; set; } = "gpt-4o-mini";

    public OpenAiProviderSettings OpenAI { get; set; } = new();

    public GeminiProviderSettings Gemini { get; set; } = new();

    public ClaudeProviderSettings Claude { get; set; } = new();
}

public class OpenAiProviderSettings
{
    public string ApiKey { get; set; } = string.Empty;

    public string BaseUrl { get; set; } = "https://api.openai.com/v1";

    public string Model { get; set; } = "gpt-4o-mini";

    public int MaxTokens { get; set; } = 4096;
}

public class GeminiProviderSettings
{
    public string ApiKey { get; set; } = string.Empty;

    public string BaseUrl { get; set; } = "https://generativelanguage.googleapis.com/v1beta";

    public string Model { get; set; } = "gemini-1.5-flash";

    public int MaxTokens { get; set; } = 4096;
}

public class ClaudeProviderSettings
{
    public string ApiKey { get; set; } = string.Empty;

    public string BaseUrl { get; set; } = "https://api.anthropic.com/v1";

    public string Model { get; set; } = "claude-3-5-sonnet-20241022";

    public int MaxTokens { get; set; } = 4096;
}
