using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using AutoWork.Application.Common.Exceptions;
using AutoWork.Application.DTOs.Zalo;
using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Application.Interfaces.Services;
using AutoWork.Domain.Entities;
using AutoWork.Infrastructure.Services;
using AutoWork.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AutoWork.Infrastructure.Integrations;

public class ZaloService : IZaloService
{
    private readonly HttpClient _httpClient;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IntegrationTokenStore _tokenStore;
    private readonly ZaloSettings _settings;
    private readonly ILogger<ZaloService> _logger;

    public ZaloService(
        HttpClient httpClient,
        IUnitOfWork unitOfWork,
        IntegrationTokenStore tokenStore,
        IOptions<ZaloSettings> settings,
        ILogger<ZaloService> logger)
    {
        _httpClient = httpClient;
        _unitOfWork = unitOfWork;
        _tokenStore = tokenStore;
        _settings = settings.Value;
        _logger = logger;
    }

    public Task<string> GetAuthorizationUrlAsync(Guid userId, string redirectUri, CancellationToken cancellationToken = default)
    {
        var scopes = string.Join(",", _settings.Scopes);
        var url = $"{_settings.OAuthBaseUrl.TrimEnd('/')}/permission" +
                  $"?app_id={Uri.EscapeDataString(_settings.AppId)}" +
                  $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                  $"&state={Uri.EscapeDataString(userId.ToString())}" +
                  $"&scope={Uri.EscapeDataString(scopes)}";
        return Task.FromResult(url);
    }

    public async Task<ZaloAccountDto> ConnectAccountAsync(Guid userId, string authorizationCode, CancellationToken cancellationToken = default)
    {
        var tokenResponse = await ExchangeCodeForTokenAsync(authorizationCode, cancellationToken);
        var oaInfo = await GetOfficialAccountAsync(tokenResponse.AccessToken, cancellationToken);

        var existing = (await _unitOfWork.Zalo.GetAccountsByUserIdAsync(userId, cancellationToken))
            .FirstOrDefault(a => a.ZaloUserId == oaInfo.OaId);

        ZaloAccount account;
        if (existing is not null)
        {
            account = existing;
            account.DisplayName = oaInfo.Name;
            account.AvatarUrl = oaInfo.Avatar;
            account.IsConnected = true;
            account.TokenExpiresAt = tokenResponse.ExpiresIn.HasValue
                ? DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn.Value)
                : DateTime.UtcNow.AddDays(90);
            account.LastSyncedAt = DateTime.UtcNow;
            account.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Zalo.UpdateAsync(account, cancellationToken);
        }
        else
        {
            account = new ZaloAccount
            {
                UserId = userId,
                ZaloUserId = oaInfo.OaId,
                DisplayName = oaInfo.Name,
                AvatarUrl = oaInfo.Avatar,
                IsConnected = true,
                TokenExpiresAt = tokenResponse.ExpiresIn.HasValue
                    ? DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn.Value)
                    : DateTime.UtcNow.AddDays(90),
                LastSyncedAt = DateTime.UtcNow
            };
            await _unitOfWork.Zalo.AddAsync(account, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _tokenStore.Set(IntegrationTokenStore.ZaloAccountTokenKey(account.Id), tokenResponse.AccessToken, account.TokenExpiresAt);

        return new ZaloAccountDto
        {
            Id = account.Id,
            OaId = account.ZaloUserId,
            OaName = account.DisplayName,
            IsActive = account.IsConnected,
            TokenExpiresAt = account.TokenExpiresAt
        };
    }

    public async Task<string> PublishPostAsync(Guid userId, ZaloPostDto request, CancellationToken cancellationToken = default)
    {
        var account = await _unitOfWork.Zalo.GetByIdAsync(request.ZaloAccountId, cancellationToken)
            ?? throw new NotFoundException("ZaloAccount", request.ZaloAccountId);

        if (account.UserId != userId)
        {
            throw new UnauthorizedAccessException();
        }

        var accessToken = _tokenStore.Get(IntegrationTokenStore.ZaloAccountTokenKey(account.Id));
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            throw new BadRequestException("Zalo access token expired. Reconnect the account.");
        }

        var payload = new Dictionary<string, object?>
        {
            ["type"] = string.IsNullOrWhiteSpace(request.MediaUrl) ? "normal" : "photo",
            ["title"] = request.Title,
            ["author"] = account.DisplayName,
            ["cover"] = new { photo_url = request.MediaUrl ?? string.Empty },
            ["description"] = request.Content,
            ["body"] = new[] { new { type = "text", content = request.Content } },
            ["status"] = request.ScheduledAt.HasValue && request.ScheduledAt.Value > DateTime.UtcNow ? "hide" : "show"
        };

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{_settings.OaApiBaseUrl.TrimEnd('/')}/article/create");
        httpRequest.Headers.Add("access_token", accessToken);
        httpRequest.Content = JsonContent.Create(payload);

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Zalo publish failed for account {AccountId}: {Body}", account.Id, body);
            throw new BadRequestException("Failed to publish Zalo post.");
        }

        var result = JsonSerializer.Deserialize<ZaloArticleResponse>(body);
        if (result?.Error != 0)
        {
            _logger.LogError("Zalo publish returned error {Error}: {Message}", result?.Error, result?.Message);
            throw new BadRequestException(result?.Message ?? "Failed to publish Zalo post.");
        }

        var externalId = result?.Data?.Id ?? Guid.NewGuid().ToString("N");
        var post = new ZaloPost
        {
            ZaloAccountId = account.Id,
            ExternalPostId = externalId,
            Content = request.Content,
            Status = 1,
            PublishedAt = request.ScheduledAt ?? DateTime.UtcNow
        };

        await _unitOfWork.Zalo.AddPostAsync(post, cancellationToken);
        account.LastSyncedAt = DateTime.UtcNow;
        account.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Zalo.UpdateAsync(account, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return externalId;
    }

    private async Task<ZaloTokenResponse> ExchangeCodeForTokenAsync(string code, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"{_settings.OAuthBaseUrl.TrimEnd('/')}/access_token");
        request.Headers.Add("secret_key", _settings.AppSecret);
        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["app_id"] = _settings.AppId,
            ["code"] = code,
            ["grant_type"] = "authorization_code"
        });

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Zalo token exchange failed: {Body}", body);
            throw new BadRequestException("Failed to exchange Zalo authorization code.");
        }

        var token = JsonSerializer.Deserialize<ZaloTokenResponse>(body);
        if (token is null || string.IsNullOrWhiteSpace(token.AccessToken))
        {
            throw new BadRequestException("Zalo access token missing.");
        }

        return token;
    }

    private async Task<ZaloOaInfo> GetOfficialAccountAsync(string accessToken, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync(
            $"{_settings.OaApiBaseUrl.TrimEnd('/')}/getoa?access_token={Uri.EscapeDataString(accessToken)}",
            cancellationToken);

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Zalo OA info fetch failed: {Body}", body);
            throw new BadRequestException("Failed to load Zalo official account.");
        }

        var result = JsonSerializer.Deserialize<ZaloOaResponse>(body);
        if (result?.Error != 0 || result.Data is null)
        {
            throw new BadRequestException(result?.Message ?? "Invalid Zalo official account response.");
        }

        return result.Data;
    }

    private sealed class ZaloTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("expires_in")]
        public int? ExpiresIn { get; set; }
    }

    private sealed class ZaloOaResponse
    {
        [JsonPropertyName("error")]
        public int Error { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("data")]
        public ZaloOaInfo? Data { get; set; }
    }

    private sealed class ZaloOaInfo
    {
        [JsonPropertyName("oa_id")]
        public string OaId { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("avatar")]
        public string? Avatar { get; set; }
    }

    private sealed class ZaloArticleResponse
    {
        [JsonPropertyName("error")]
        public int Error { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("data")]
        public ZaloArticleData? Data { get; set; }
    }

    private sealed class ZaloArticleData
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }
    }
}
