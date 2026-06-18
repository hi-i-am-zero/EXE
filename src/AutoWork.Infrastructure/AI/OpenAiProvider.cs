using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AutoWork.Infrastructure.Settings;
using AutoWork.Shared.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AutoWork.Infrastructure.AI;

public class OpenAiProvider : IAiProvider
{
    private readonly HttpClient _httpClient;
    private readonly OpenAiProviderSettings _settings;
    private readonly ILogger<OpenAiProvider> _logger;

    public OpenAiProvider(HttpClient httpClient, IOptions<AiSettings> settings, ILogger<OpenAiProvider> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value.OpenAI;
        _logger = logger;
    }

    public AiProvider Provider => AiProvider.OpenAI;

    public async Task<AiGenerationResult> GenerateAsync(AiGenerationRequest request, CancellationToken cancellationToken = default)
    {
        ValidateApiKey();

        var payload = new
        {
            model = _settings.Model,
            max_tokens = request.MaxTokens > 0 ? request.MaxTokens : _settings.MaxTokens,
            temperature = 0.7,
            response_format = new { type = "json_object" },
            messages = new object[]
            {
                new { role = "system", content = request.SystemPrompt },
                new { role = "user", content = request.UserPrompt }
            }
        };

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{_settings.BaseUrl.TrimEnd('/')}/chat/completions");
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiKey);
        httpRequest.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("OpenAI API error: {StatusCode} {Body}", response.StatusCode, body);
            throw new InvalidOperationException($"OpenAI API request failed: {response.StatusCode}");
        }

        var parsed = JsonSerializer.Deserialize<OpenAiChatResponse>(body);
        var content = parsed?.Choices?.FirstOrDefault()?.Message?.Content ?? string.Empty;
        var tokensUsed = parsed?.Usage?.TotalTokens ?? 0;

        return AiResponseParser.ParseStructuredResponse(content, tokensUsed);
    }

    private void ValidateApiKey()
    {
        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            throw new InvalidOperationException("OpenAI API key is not configured.");
        }
    }

    private sealed class OpenAiChatResponse
    {
        [JsonPropertyName("choices")]
        public List<OpenAiChoice>? Choices { get; set; }

        [JsonPropertyName("usage")]
        public OpenAiUsage? Usage { get; set; }
    }

    private sealed class OpenAiChoice
    {
        [JsonPropertyName("message")]
        public OpenAiMessage? Message { get; set; }
    }

    private sealed class OpenAiMessage
    {
        [JsonPropertyName("content")]
        public string? Content { get; set; }
    }

    private sealed class OpenAiUsage
    {
        [JsonPropertyName("total_tokens")]
        public int TotalTokens { get; set; }
    }
}
