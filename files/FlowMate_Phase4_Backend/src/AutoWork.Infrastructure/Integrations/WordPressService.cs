using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AutoWork.Application.Common.Exceptions;
using AutoWork.Application.DTOs.WordPress;
using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Application.Interfaces.Services;
using AutoWork.Domain.Entities;
using AutoWork.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AutoWork.Infrastructure.Integrations;

public class WordPressService : IWordPressService
{
    private readonly HttpClient _httpClient;
    private readonly IUnitOfWork _unitOfWork;
    private readonly WordPressSettings _settings;
    private readonly ILogger<WordPressService> _logger;

    public WordPressService(
        HttpClient httpClient,
        IUnitOfWork unitOfWork,
        IOptions<WordPressSettings> settings,
        ILogger<WordPressService> logger)
    {
        _httpClient = httpClient;
        _unitOfWork = unitOfWork;
        _settings = settings.Value;
        _logger = logger;
        _httpClient.Timeout = TimeSpan.FromSeconds(_settings.RequestTimeoutSeconds);
    }

    public async Task<bool> TestConnectionAsync(WordPressSiteDto site, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(site.SiteUrl) ||
            string.IsNullOrWhiteSpace(site.Username) ||
            string.IsNullOrWhiteSpace(site.ApplicationPassword))
        {
            return false;
        }

        try
        {
            using var request = CreateAuthorizedRequest(HttpMethod.Get, site.SiteUrl, site.Username, site.ApplicationPassword, "users/me?context=edit");
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "WordPress connection test failed for {SiteUrl}", site.SiteUrl);
            return false;
        }
    }

    public async Task<string> PublishPostAsync(Guid userId, CreateWordPressPostDto request, CancellationToken cancellationToken = default)
    {
        var site = await _unitOfWork.WordPress.GetByIdAsync(request.WordPressSiteId, cancellationToken)
            ?? throw new NotFoundException("WordPressSite", request.WordPressSiteId);

        if (site.UserId != userId)
        {
            throw new UnauthorizedAccessException();
        }

        var payload = new Dictionary<string, object?>
        {
            ["title"] = request.Title,
            ["content"] = request.Content,
            ["status"] = string.IsNullOrWhiteSpace(request.Status) ? _settings.DefaultPostStatus : request.Status
        };

        if (!string.IsNullOrWhiteSpace(request.Slug))
        {
            payload["slug"] = request.Slug;
        }

        if (request.ScheduledAt.HasValue && request.ScheduledAt.Value > DateTime.UtcNow)
        {
            payload["status"] = "future";
            payload["date"] = request.ScheduledAt.Value.ToUniversalTime().ToString("o");
        }

        using var httpRequest = CreateAuthorizedRequest(HttpMethod.Post, site.SiteUrl, site.Username, site.ApplicationPassword, "posts");
        httpRequest.Content = JsonContent.Create(payload);

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("WordPress publish failed for site {SiteId}: {Body}", site.Id, body);
            throw new BadRequestException("Failed to publish WordPress post.");
        }

        var created = JsonSerializer.Deserialize<WordPressApiPost>(body)
            ?? throw new BadRequestException("Invalid WordPress publish response.");

        var wpPost = new WordPressPost
        {
            WordPressSiteId = site.Id,
            ExternalPostId = created.Id.ToString(),
            Title = request.Title,
            Excerpt = request.Content,
            Status = MapStatusToInt(created.Status),
            Permalink = created.Link,
            PublishedAt = created.Status == "publish" ? DateTime.UtcNow : null
        };

        await _unitOfWork.WordPress.AddPostAsync(wpPost, cancellationToken);
        site.LastSyncedAt = DateTime.UtcNow;
        site.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.WordPress.UpdateAsync(site, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return created.Link ?? created.Id.ToString();
    }

    public async Task<WordPressPostDto> GetPostAsync(Guid siteId, string externalPostId, CancellationToken cancellationToken = default)
    {
        var site = await _unitOfWork.WordPress.GetByIdAsync(siteId, cancellationToken)
            ?? throw new NotFoundException("WordPressSite", siteId);

        using var request = CreateAuthorizedRequest(HttpMethod.Get, site.SiteUrl, site.Username, site.ApplicationPassword, $"posts/{externalPostId}");
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("WordPress get post failed for site {SiteId}: {Body}", siteId, body);
            throw new NotFoundException("WordPressPost", externalPostId);
        }

        var post = JsonSerializer.Deserialize<WordPressApiPost>(body)
            ?? throw new BadRequestException("Invalid WordPress post response.");

        return new WordPressPostDto
        {
            WordPressSiteId = siteId,
            Title = post.Title?.Rendered ?? string.Empty,
            Content = post.Content?.Rendered ?? string.Empty,
            Status = post.Status ?? string.Empty,
            ExternalPostId = post.Id.ToString(),
            Slug = post.Slug,
            CreatedAt = post.Date ?? DateTime.UtcNow
        };
    }

    private static HttpRequestMessage CreateAuthorizedRequest(
        HttpMethod method,
        string siteUrl,
        string username,
        string applicationPassword,
        string relativePath)
    {
        var request = new HttpRequestMessage(method, BuildApiUrl(siteUrl, relativePath));
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{applicationPassword}"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return request;
    }

    private static string BuildApiUrl(string siteUrl, string relativePath) =>
        $"{siteUrl.TrimEnd('/')}/wp-json/wp/v2/{relativePath.TrimStart('/')}";

    private static int MapStatusToInt(string? status) =>
        status?.ToLowerInvariant() switch
        {
            "publish" => 2,
            "future" => 3,
            "draft" => 1,
            _ => 0
        };

    private sealed class WordPressApiPost
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("link")]
        public string? Link { get; set; }

        [JsonPropertyName("slug")]
        public string? Slug { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("date")]
        public DateTime? Date { get; set; }

        [JsonPropertyName("title")]
        public WordPressRenderedField? Title { get; set; }

        [JsonPropertyName("content")]
        public WordPressRenderedField? Content { get; set; }
    }

    private sealed class WordPressRenderedField
    {
        [JsonPropertyName("rendered")]
        public string? Rendered { get; set; }
    }
}
