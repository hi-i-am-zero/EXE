using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using AutoWork.Application.Common.Exceptions;
using AutoWork.Application.DTOs.Facebook;
using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Application.Interfaces.Services;
using AutoWork.Domain.Entities;
using AutoWork.Infrastructure.Services;
using AutoWork.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AutoWork.Infrastructure.Integrations;

public class FacebookService : IFacebookService
{
    private readonly HttpClient _httpClient;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IntegrationTokenStore _tokenStore;
    private readonly FacebookSettings _settings;
    private readonly ILogger<FacebookService> _logger;

    public FacebookService(
        HttpClient httpClient,
        IUnitOfWork unitOfWork,
        IntegrationTokenStore tokenStore,
        IOptions<FacebookSettings> settings,
        ILogger<FacebookService> logger)
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
        var url = $"https://www.facebook.com/{_settings.GraphApiVersion}/dialog/oauth" +
                  $"?client_id={Uri.EscapeDataString(_settings.AppId)}" +
                  $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                  $"&state={Uri.EscapeDataString(userId.ToString())}" +
                  $"&scope={Uri.EscapeDataString(scopes)}" +
                  "&response_type=code";
        return Task.FromResult(url);
    }

    public async Task<FacebookAccountDto> ConnectAccountAsync(
        Guid userId,
        string authorizationCode,
        string redirectUri,
        CancellationToken cancellationToken = default)
    {
        var shortToken = await ExchangeCodeForTokenAsync(authorizationCode, redirectUri, cancellationToken);
        var tokenResponse = await GetLongLivedUserTokenAsync(shortToken, cancellationToken);
        var profile = await GetUserProfileAsync(tokenResponse.AccessToken, cancellationToken);

        var existing = (await _unitOfWork.Facebook.GetAccountsByUserIdAsync(userId, cancellationToken))
            .FirstOrDefault(a => a.FacebookUserId == profile.Id);

        FacebookAccount account;
        if (existing is not null)
        {
            account = existing;
            account.Name = profile.Name;
            account.Email = profile.Email;
            account.ProfilePictureUrl = profile.Picture?.Data?.Url;
            account.IsActive = true;
            account.TokenExpiresAt = tokenResponse.ExpiresIn.HasValue
                ? DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn.Value)
                : DateTime.UtcNow.AddDays(60);
            account.LastSyncedAt = DateTime.UtcNow;
            account.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Facebook.UpdateAsync(account, cancellationToken);
        }
        else
        {
            account = new FacebookAccount
            {
                UserId = userId,
                FacebookUserId = profile.Id,
                Name = profile.Name,
                Email = profile.Email,
                ProfilePictureUrl = profile.Picture?.Data?.Url,
                IsActive = true,
                TokenExpiresAt = tokenResponse.ExpiresIn.HasValue
                    ? DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn.Value)
                    : DateTime.UtcNow.AddDays(60),
                LastSyncedAt = DateTime.UtcNow
            };
            await _unitOfWork.Facebook.AddAsync(account, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _tokenStore.Set(IntegrationTokenStore.FacebookUserTokenKey(account.Id), tokenResponse.AccessToken, account.TokenExpiresAt);

        var pages = await SyncPagesInternalAsync(account, tokenResponse.AccessToken, cancellationToken);
        return MapAccount(account, pages);
    }

    public async Task<string> PublishPostAsync(Guid userId, PublishFacebookPostDto request, CancellationToken cancellationToken = default)
    {
        var page = await _TestPageAccessAsync(userId, request.FacebookPageId, cancellationToken);
        var pageToken = _tokenStore.Get(IntegrationTokenStore.FacebookPageTokenKey(page.Id))
            ?? _tokenStore.Get(IntegrationTokenStore.FacebookUserTokenKey(page.FacebookAccountId))
            ?? throw new BadRequestException("Facebook page token is missing. Sync pages again.");

        var payload = new Dictionary<string, object?>
        {
            ["message"] = request.Message,
            ["access_token"] = pageToken
        };

        if (!string.IsNullOrWhiteSpace(request.Link))
        {
            payload["link"] = request.Link;
        }

        if (request.ScheduledAt.HasValue && request.ScheduledAt.Value > DateTime.UtcNow.AddMinutes(10))
        {
            payload["published"] = false;
            payload["scheduled_publish_time"] = new DateTimeOffset(request.ScheduledAt.Value).ToUnixTimeSeconds();
        }

        using var response = await _httpClient.PostAsJsonAsync(
            GraphUrl($"{page.PageId}/feed"),
            payload,
            cancellationToken);

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Facebook publish failed for page {PageId}: {Body}", page.PageId, body);
            throw new BadRequestException("Failed to publish Facebook post.");
        }

        var result = JsonSerializer.Deserialize<FacebookPublishResponse>(body);
        return result?.Id ?? throw new BadRequestException("Facebook did not return a post id.");
    }

    public async Task<IReadOnlyList<FacebookPageDto>> SyncPagesAsync(Guid userId, Guid accountId, CancellationToken cancellationToken = default)
    {
        var account = await _unitOfWork.Facebook.GetAccountWithPagesAsync(accountId, cancellationToken)
            ?? throw new NotFoundException("FacebookAccount", accountId);

        if (account.UserId != userId)
        {
            throw new UnauthorizedAccessException();
        }

        var userToken = _tokenStore.Get(IntegrationTokenStore.FacebookUserTokenKey(account.Id));
        if (string.IsNullOrWhiteSpace(userToken))
        {
            throw new BadRequestException("Facebook account token expired. Reconnect the account.");
        }

        var pages = await SyncPagesInternalAsync(account, userToken, cancellationToken);
        return pages;
    }

    private async Task<IReadOnlyList<FacebookPageDto>> SyncPagesInternalAsync(
        FacebookAccount account,
        string userAccessToken,
        CancellationToken cancellationToken)
    {
        var graphPages = await GetManagedPagesAsync(userAccessToken, cancellationToken);
        var synced = new List<FacebookPageDto>();

        foreach (var graphPage in graphPages)
        {
            var page = account.Pages.FirstOrDefault(p => p.PageId == graphPage.Id);
            if (page is null)
            {
                page = new FacebookPage
                {
                    FacebookAccountId = account.Id,
                    PageId = graphPage.Id,
                    Name = graphPage.Name,
                    Category = graphPage.Category,
                    ProfilePictureUrl = graphPage.Picture?.Data?.Url,
                    IsConnected = true,
                    LastSyncedAt = DateTime.UtcNow
                };
                await _unitOfWork.Facebook.AddPageAsync(page, cancellationToken);
                account.Pages.Add(page);
            }
            else
            {
                page.Name = graphPage.Name;
                page.Category = graphPage.Category;
                page.ProfilePictureUrl = graphPage.Picture?.Data?.Url;
                page.IsConnected = true;
                page.LastSyncedAt = DateTime.UtcNow;
                page.UpdatedAt = DateTime.UtcNow;
            }

            if (!string.IsNullOrWhiteSpace(graphPage.AccessToken))
            {
                _tokenStore.Set(IntegrationTokenStore.FacebookPageTokenKey(page.Id), graphPage.AccessToken);
            }

            synced.Add(MapPage(page));
        }

        account.LastSyncedAt = DateTime.UtcNow;
        account.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Facebook.UpdateAsync(account, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return synced;
    }

    private async Task<FacebookPage> _TestPageAccessAsync(Guid userId, Guid pageId, CancellationToken cancellationToken)
    {
        var page = await _unitOfWork.Facebook.GetPageByIdAsync(pageId, cancellationToken)
            ?? throw new NotFoundException("FacebookPage", pageId);

        var account = await _unitOfWork.Facebook.GetByIdAsync(page.FacebookAccountId, cancellationToken);
        if (account is null || account.UserId != userId)
        {
            throw new UnauthorizedAccessException();
        }

        return page;
    }

    private async Task<string> ExchangeCodeForTokenAsync(string code, string redirectUri, CancellationToken cancellationToken)
    {
        var url = GraphUrl("oauth/access_token") +
                  $"?client_id={Uri.EscapeDataString(_settings.AppId)}" +
                  $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                  $"&client_secret={Uri.EscapeDataString(_settings.AppSecret)}" +
                  $"&code={Uri.EscapeDataString(code)}";

        using var response = await _httpClient.GetAsync(url, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Facebook token exchange failed: {Body}", body);
            throw new BadRequestException("Failed to exchange Facebook authorization code.");
        }

        var token = JsonSerializer.Deserialize<FacebookTokenResponse>(body);
        return token?.AccessToken ?? throw new BadRequestException("Facebook access token missing.");
    }

    private async Task<FacebookTokenResponse> GetLongLivedUserTokenAsync(string shortLivedToken, CancellationToken cancellationToken)
    {
        var url = GraphUrl("oauth/access_token") +
                  $"?grant_type=fb_exchange_token" +
                  $"&client_id={Uri.EscapeDataString(_settings.AppId)}" +
                  $"&client_secret={Uri.EscapeDataString(_settings.AppSecret)}" +
                  $"&fb_exchange_token={Uri.EscapeDataString(shortLivedToken)}";

        using var response = await _httpClient.GetAsync(url, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Facebook long-lived token exchange failed, using short-lived token: {Body}", body);
            return new FacebookTokenResponse { AccessToken = shortLivedToken };
        }

        return JsonSerializer.Deserialize<FacebookTokenResponse>(body)
            ?? new FacebookTokenResponse { AccessToken = shortLivedToken };
    }

    private async Task<FacebookUserResponse> GetUserProfileAsync(string accessToken, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync(
            GraphUrl("me?fields=id,name,email,picture") + $"&access_token={Uri.EscapeDataString(accessToken)}",
            cancellationToken);

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Facebook profile fetch failed: {Body}", body);
            throw new BadRequestException("Failed to load Facebook profile.");
        }

        return JsonSerializer.Deserialize<FacebookUserResponse>(body)
            ?? throw new BadRequestException("Invalid Facebook profile response.");
    }

    private async Task<List<FacebookGraphPage>> GetManagedPagesAsync(string accessToken, CancellationToken cancellationToken)
    {
        var pages = new List<FacebookGraphPage>();
        var nextUrl = GraphUrl("me/accounts?fields=id,name,category,picture,access_token") +
                      $"&access_token={Uri.EscapeDataString(accessToken)}";

        while (!string.IsNullOrWhiteSpace(nextUrl))
        {
            using var response = await _httpClient.GetAsync(nextUrl, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Facebook pages fetch failed: {Body}", body);
                throw new BadRequestException("Failed to sync Facebook pages.");
            }

            var pageResponse = JsonSerializer.Deserialize<FacebookPagesResponse>(body);
            if (pageResponse?.Data is not null)
            {
                pages.AddRange(pageResponse.Data);
            }

            nextUrl = pageResponse?.Paging?.Next ?? string.Empty;
        }

        return pages;
    }

    private string GraphUrl(string path) =>
        $"{_settings.GraphApiBaseUrl.TrimEnd('/')}/{_settings.GraphApiVersion}/{path.TrimStart('/')}";

    private static FacebookAccountDto MapAccount(FacebookAccount account, IReadOnlyList<FacebookPageDto> pages) =>
        new()
        {
            Id = account.Id,
            FacebookUserId = account.FacebookUserId,
            Name = account.Name,
            TokenExpiresAt = account.TokenExpiresAt,
            IsActive = account.IsActive,
            Pages = pages
        };

    private static FacebookPageDto MapPage(FacebookPage page) =>
        new()
        {
            Id = page.Id,
            FacebookAccountId = page.FacebookAccountId,
            PageId = page.PageId,
            PageName = page.Name,
            Category = page.Category,
            PictureUrl = page.ProfilePictureUrl,
            IsActive = page.IsConnected
        };

    private sealed class FacebookTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("expires_in")]
        public int? ExpiresIn { get; set; }
    }

    private sealed class FacebookUserResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("picture")]
        public FacebookPictureWrapper? Picture { get; set; }
    }

    private sealed class FacebookPictureWrapper
    {
        [JsonPropertyName("data")]
        public FacebookPictureData? Data { get; set; }
    }

    private sealed class FacebookPictureData
    {
        [JsonPropertyName("url")]
        public string? Url { get; set; }
    }

    private sealed class FacebookPagesResponse
    {
        [JsonPropertyName("data")]
        public List<FacebookGraphPage>? Data { get; set; }

        [JsonPropertyName("paging")]
        public FacebookPaging? Paging { get; set; }
    }

    private sealed class FacebookPaging
    {
        [JsonPropertyName("next")]
        public string? Next { get; set; }
    }

    private sealed class FacebookGraphPage
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("category")]
        public string? Category { get; set; }

        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }

        [JsonPropertyName("picture")]
        public FacebookPictureWrapper? Picture { get; set; }
    }

    private sealed class FacebookPublishResponse
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }
    }
}
