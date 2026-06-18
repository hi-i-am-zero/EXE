using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AutoWork.Infrastructure.Settings;
using AutoWork.Shared.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AutoWork.Infrastructure.AI;

public class ClaudeAiProvider : IAiProvider
{
    private readonly HttpClient _httpClient;
    private readonly ClaudeProviderSettings _settings;
    private readonly ILogger<ClaudeAiProvider> _logger;

    public ClaudeAiProvider(HttpClient httpClient, IOptions<AiSettings> settings, ILogger<ClaudeAiProvider> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value.Claude;
        _logger = logger;
    }

    public AiProvider Provider => AiProvider.Claude;

    public async Task<AiGenerationResult> GenerateAsync(AiGenerationRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            throw new InvalidOperationException("Claude API key is not configured.");
        }

        var payload = new
        {
            model = _settings.Model,
            max_tokens = request.MaxTokens > 0 ? request.MaxTokens : _settings.MaxTokens,
            system = request.SystemPrompt,
            messages = new[]
            {
                new { role = "user", content = request.UserPrompt }
            }
        };

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{_settings.BaseUrl.TrimEnd('/')}/messages");
        httpRequest.Headers.Add("x-api-key", _settings.ApiKey);
        httpRequest.Headers.Add("anthropic-version", "2023-06-01");
        httpRequest.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Claude API error: {StatusCode} {Body}", response.StatusCode, body);
            throw new InvalidOperationException($"Claude API request failed: {response.StatusCode}");
        }

        var parsed = JsonSerializer.Deserialize<ClaudeResponse>(body);
        var content = parsed?.Content?.FirstOrDefault()?.Text ?? string.Empty;
        var tokensUsed = (parsed?.Usage?.InputTokens ?? 0) + (parsed?.Usage?.OutputTokens ?? 0);

        return AiResponseParser.ParseStructuredResponse(content, tokensUsed);
    }

    private sealed class ClaudeResponse
    {
        [JsonPropertyName("content")]
        public List<ClaudeContentBlock>? Content { get; set; }

        [JsonPropertyName("usage")]
        public ClaudeUsage? Usage { get; set; }
    }

    private sealed class ClaudeContentBlock
    {
        [JsonPropertyName("text")]
        public string? Text { get; set; }
    }

    private sealed class ClaudeUsage
    {
        [JsonPropertyName("input_tokens")]
        public int InputTokens { get; set; }

        [JsonPropertyName("output_tokens")]
        public int OutputTokens { get; set; }
    }
}
