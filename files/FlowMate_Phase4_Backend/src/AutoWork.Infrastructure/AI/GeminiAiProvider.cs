using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AutoWork.Infrastructure.Settings;
using AutoWork.Shared.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AutoWork.Infrastructure.AI;

public class GeminiAiProvider : IAiProvider
{
    private readonly HttpClient _httpClient;
    private readonly GeminiProviderSettings _settings;
    private readonly ILogger<GeminiAiProvider> _logger;

    public GeminiAiProvider(HttpClient httpClient, IOptions<AiSettings> settings, ILogger<GeminiAiProvider> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value.Gemini;
        _logger = logger;
    }

    public AiProvider Provider => AiProvider.Gemini;

    public async Task<AiGenerationResult> GenerateAsync(AiGenerationRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            throw new InvalidOperationException("Gemini API key is not configured.");
        }

        var url = $"{_settings.BaseUrl.TrimEnd('/')}/models/{_settings.Model}:generateContent?key={_settings.ApiKey}";
        var payload = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = $"{request.SystemPrompt}\n\n{request.UserPrompt}" }
                    }
                }
            },
            generationConfig = new
            {
                maxOutputTokens = request.MaxTokens > 0 ? request.MaxTokens : _settings.MaxTokens,
                temperature = 0.7,
                responseMimeType = "application/json"
            }
        };

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
        };

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Gemini API error: {StatusCode} {Body}", response.StatusCode, body);
            throw new InvalidOperationException($"Gemini API request failed: {response.StatusCode}");
        }

        var parsed = JsonSerializer.Deserialize<GeminiResponse>(body);
        var content = parsed?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text ?? string.Empty;
        var tokensUsed = parsed?.UsageMetadata?.TotalTokenCount ?? 0;

        return AiResponseParser.ParseStructuredResponse(content, tokensUsed);
    }

    private sealed class GeminiResponse
    {
        [JsonPropertyName("candidates")]
        public List<GeminiCandidate>? Candidates { get; set; }

        [JsonPropertyName("usageMetadata")]
        public GeminiUsage? UsageMetadata { get; set; }
    }

    private sealed class GeminiCandidate
    {
        [JsonPropertyName("content")]
        public GeminiContent? Content { get; set; }
    }

    private sealed class GeminiContent
    {
        [JsonPropertyName("parts")]
        public List<GeminiPart>? Parts { get; set; }
    }

    private sealed class GeminiPart
    {
        [JsonPropertyName("text")]
        public string? Text { get; set; }
    }

    private sealed class GeminiUsage
    {
        [JsonPropertyName("totalTokenCount")]
        public int TotalTokenCount { get; set; }
    }
}
