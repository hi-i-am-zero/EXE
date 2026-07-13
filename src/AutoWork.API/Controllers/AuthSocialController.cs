using System.Text.Json;
using AutoWork.Application.Common.Helpers;
using AutoWork.Application.DTOs.Auth;
using AutoWork.Application.Interfaces.Services;
using AutoWork.Domain.Entities;
using AutoWork.Infrastructure.Settings;
using AutoWork.Persistence.Context;
using AutoWork.Shared.Enums;
using AutoWork.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text.Json.Serialization;

namespace AutoWork.API.Controllers;

[Route("api/auth/social")]
public class AuthSocialController : ApiControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly GoogleSettings _googleSettings;
    private readonly FacebookSettings _facebookSettings;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public AuthSocialController(
        ApplicationDbContext context,
        IJwtTokenService jwtTokenService,
        IHttpClientFactory httpClientFactory,
        IOptions<GoogleSettings> googleSettings,
        IOptions<FacebookSettings> facebookSettings)
    {
        _context = context;
        _jwtTokenService = jwtTokenService;
        _httpClientFactory = httpClientFactory;
        _googleSettings = googleSettings.Value;
        _facebookSettings = facebookSettings.Value;
    }

    [HttpGet("config")]
    [AllowAnonymous]
    public ActionResult<ApiResponse<SocialAuthConfigResponse>> GetConfig()
    {
        return OkResponse(new SocialAuthConfigResponse
        {
            GoogleClientId = _googleSettings.ClientId,
            FacebookAppId = _facebookSettings.AppId,
            FacebookApiVersion = _facebookSettings.GraphApiVersion
        });
    }

    [HttpPost("google")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Google([FromBody] GoogleSocialLoginRequest request)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.IdToken))
        {
            return FailResponse<AuthResponse>("Google idToken is required.");
        }

        if (string.IsNullOrWhiteSpace(_googleSettings.ClientId))
        {
            return FailResponse<AuthResponse>("Google OAuth is not configured on server.");
        }

        var profile = await ResolveGoogleProfileAsync(request.IdToken);
        if (profile is null)
        {
            return FailResponse<AuthResponse>("Google login failed. Invalid token or app configuration mismatch.");
        }

        var auth = await SignInOrSignUpAsync("google", profile.ProviderId, profile.Email, profile.FirstName, profile.LastName);
        return OkResponse(auth, "Google login successful.");
    }

    [HttpPost("facebook")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Facebook([FromBody] FacebookSocialLoginRequest request)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.AccessToken))
        {
            return FailResponse<AuthResponse>("Facebook accessToken is required.");
        }

        if (string.IsNullOrWhiteSpace(_facebookSettings.AppId) || string.IsNullOrWhiteSpace(_facebookSettings.AppSecret))
        {
            return FailResponse<AuthResponse>("Facebook OAuth is not configured on server.");
        }

        var profile = await ResolveFacebookProfileAsync(request.AccessToken);
        if (profile is null)
        {
            return FailResponse<AuthResponse>("Facebook login failed. Invalid token or app configuration mismatch.");
        }

        var auth = await SignInOrSignUpAsync("facebook", profile.ProviderId, profile.Email, profile.FirstName, profile.LastName);
        return OkResponse(auth, "Facebook login successful.");
    }

    private async Task<SocialProfile?> ResolveGoogleProfileAsync(string idToken)
    {
        var client = _httpClientFactory.CreateClient();
        var url = $"https://oauth2.googleapis.com/tokeninfo?id_token={Uri.EscapeDataString(idToken)}";
        var response = await client.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var json = await response.Content.ReadAsStringAsync();
        var tokenInfo = JsonSerializer.Deserialize<GoogleTokenInfo>(json, JsonOptions);
        if (tokenInfo is null || string.IsNullOrWhiteSpace(tokenInfo.Sub))
        {
            return null;
        }

        if (!string.Equals(tokenInfo.Aud, _googleSettings.ClientId, StringComparison.Ordinal))
        {
            return null;
        }

        if (!string.Equals(tokenInfo.EmailVerified, "true", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        if (!IsValidGoogleIssuer(tokenInfo.Iss))
        {
            return null;
        }

        if (long.TryParse(tokenInfo.Exp, out var expSeconds))
        {
            var expiresAt = DateTimeOffset.FromUnixTimeSeconds(expSeconds);
            if (expiresAt <= DateTimeOffset.UtcNow)
            {
                return null;
            }
        }

        var email = string.IsNullOrWhiteSpace(tokenInfo.Email)
            ? $"google_{tokenInfo.Sub}@social.local"
            : tokenInfo.Email.Trim().ToLowerInvariant();

        var firstName = tokenInfo.GivenName ?? "Google";
        var lastName = tokenInfo.FamilyName ?? "User";
        if ((tokenInfo.GivenName is null || tokenInfo.FamilyName is null) && !string.IsNullOrWhiteSpace(tokenInfo.Name))
        {
            var (splitFirst, splitLast) = SplitName(tokenInfo.Name);
            firstName = splitFirst;
            lastName = splitLast;
        }

        return new SocialProfile(
            ProviderId: tokenInfo.Sub,
            Email: email,
            FirstName: firstName,
            LastName: lastName);
    }

    private async Task<SocialProfile?> ResolveFacebookProfileAsync(string accessToken)
    {
        var client = _httpClientFactory.CreateClient();
        var appAccessToken = $"{_facebookSettings.AppId}|{_facebookSettings.AppSecret}";
        var debugUrl = GraphUrl("debug_token") +
                       $"?input_token={Uri.EscapeDataString(accessToken)}" +
                       $"&access_token={Uri.EscapeDataString(appAccessToken)}";

        var debugResponse = await client.GetAsync(debugUrl);
        if (!debugResponse.IsSuccessStatusCode)
        {
            return null;
        }

        var debugJson = await debugResponse.Content.ReadAsStringAsync();
        var debug = JsonSerializer.Deserialize<FacebookDebugTokenResponse>(debugJson, JsonOptions);
        var debugData = debug?.Data;
        if (debugData is null ||
            !debugData.IsValid ||
            !string.Equals(debugData.AppId, _facebookSettings.AppId, StringComparison.Ordinal) ||
            string.IsNullOrWhiteSpace(debugData.UserId))
        {
            return null;
        }

        var profileUrl = GraphUrl("me") +
                         $"?fields=id,email,first_name,last_name,name&access_token={Uri.EscapeDataString(accessToken)}";
        var profileResponse = await client.GetAsync(profileUrl);
        if (!profileResponse.IsSuccessStatusCode)
        {
            return null;
        }

        var profileJson = await profileResponse.Content.ReadAsStringAsync();
        var me = JsonSerializer.Deserialize<FacebookUserInfo>(profileJson, JsonOptions);
        if (me is null || string.IsNullOrWhiteSpace(me.Id))
        {
            return null;
        }

        if (!string.Equals(me.Id, debugData.UserId, StringComparison.Ordinal))
        {
            return null;
        }

        var email = string.IsNullOrWhiteSpace(me.Email)
            ? $"facebook_{me.Id}@social.local"
            : me.Email.Trim().ToLowerInvariant();

        var firstName = me.FirstName ?? "Facebook";
        var lastName = me.LastName ?? "User";
        if ((me.FirstName is null || me.LastName is null) && !string.IsNullOrWhiteSpace(me.Name))
        {
            var (splitFirst, splitLast) = SplitName(me.Name);
            firstName = splitFirst;
            lastName = splitLast;
        }

        return new SocialProfile(
            ProviderId: me.Id,
            Email: email,
            FirstName: firstName,
            LastName: lastName);
    }

    private async Task<AuthResponse> SignInOrSignUpAsync(
        string provider,
        string providerId,
        string email,
        string firstName,
        string lastName)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u =>
                u.Email == normalizedEmail ||
                (provider == "google" && u.GoogleId == providerId) ||
                (provider == "facebook" && u.FacebookId == providerId));

        if (user is null)
        {
            user = new User
            {
                Id = Guid.NewGuid(),
                Email = normalizedEmail,
                PasswordHash = PasswordHelper.Hash(Guid.NewGuid().ToString("N")),
                FirstName = string.IsNullOrWhiteSpace(firstName) ? "Social" : firstName.Trim(),
                LastName = string.IsNullOrWhiteSpace(lastName) ? "User" : lastName.Trim(),
                IsActive = true,
                ReferralCode = GenerateReferralCode()
            };

            if (provider == "google") user.GoogleId = providerId;
            if (provider == "facebook") user.FacebookId = providerId;

            await _context.Users.AddAsync(user);
            await _context.Credits.AddAsync(new Credit
            {
                UserId = user.Id,
                Balance = 100,
                TotalEarned = 100,
                TotalUsed = 0
            });
        }
        else
        {
            if (provider == "google" && string.IsNullOrWhiteSpace(user.GoogleId)) user.GoogleId = providerId;
            if (provider == "facebook" && string.IsNullOrWhiteSpace(user.FacebookId)) user.FacebookId = providerId;
            if (string.IsNullOrWhiteSpace(user.FirstName) && !string.IsNullOrWhiteSpace(firstName)) user.FirstName = firstName.Trim();
            if (string.IsNullOrWhiteSpace(user.LastName) && !string.IsNullOrWhiteSpace(lastName)) user.LastName = lastName.Trim();
        }

        user.LastLoginAt = DateTime.UtcNow;

        var roles = user.UserRoles
            .Select(ur => ur.Role.Name)
            .Where(role => !string.IsNullOrWhiteSpace(role))
            .Distinct()
            .ToList();

        if (roles.Count == 0)
        {
            roles.Add(UserRoleType.User.ToString());
        }

        var accessToken = _jwtTokenService.GenerateAccessToken(user, roles);
        var refreshTokenValue = _jwtTokenService.GenerateRefreshToken();

        await _context.RefreshTokens.AddAsync(new RefreshToken
        {
            UserId = user.Id,
            Token = refreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        });

        await _context.SaveChangesAsync();

        return new AuthResponse
        {
            UserId = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            AccessToken = accessToken,
            RefreshToken = refreshTokenValue,
            ExpiresAt = _jwtTokenService.GetAccessTokenExpiration(),
            Roles = roles
        };
    }

    private static string GenerateReferralCode()
    {
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray())
            .Replace("+", string.Empty)
            .Replace("/", string.Empty)
            .Replace("=", string.Empty)[..8]
            .ToUpperInvariant();
    }

    private string GraphUrl(string path)
    {
        var version = string.IsNullOrWhiteSpace(_facebookSettings.GraphApiVersion)
            ? "v21.0"
            : _facebookSettings.GraphApiVersion;
        var baseUrl = string.IsNullOrWhiteSpace(_facebookSettings.GraphApiBaseUrl)
            ? "https://graph.facebook.com"
            : _facebookSettings.GraphApiBaseUrl.TrimEnd('/');

        return $"{baseUrl}/{version}/{path.TrimStart('/')}";
    }

    private static bool IsValidGoogleIssuer(string? issuer)
    {
        return string.Equals(issuer, "https://accounts.google.com", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(issuer, "accounts.google.com", StringComparison.OrdinalIgnoreCase);
    }

    private static (string FirstName, string LastName) SplitName(string fullName)
    {
        var parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length <= 1)
        {
            return (fullName.Trim(), "User");
        }

        var first = string.Join(' ', parts.Take(parts.Length - 1));
        var last = parts[^1];
        return (first, last);
    }

    private sealed record SocialProfile(string ProviderId, string Email, string FirstName, string LastName);

    public sealed class GoogleSocialLoginRequest
    {
        public string IdToken { get; set; } = string.Empty;
    }

    public sealed class FacebookSocialLoginRequest
    {
        public string AccessToken { get; set; } = string.Empty;
    }

    public sealed class SocialAuthConfigResponse
    {
        public string GoogleClientId { get; set; } = string.Empty;
        public string FacebookAppId { get; set; } = string.Empty;
        public string FacebookApiVersion { get; set; } = "v21.0";
    }

    private sealed class GoogleTokenInfo
    {
        [JsonPropertyName("sub")]
        public string? Sub { get; set; }
        [JsonPropertyName("email")]
        public string? Email { get; set; }
        [JsonPropertyName("email_verified")]
        public string? EmailVerified { get; set; }
        [JsonPropertyName("aud")]
        public string? Aud { get; set; }
        [JsonPropertyName("iss")]
        public string? Iss { get; set; }
        [JsonPropertyName("exp")]
        public string? Exp { get; set; }
        [JsonPropertyName("name")]
        public string? Name { get; set; }
        [JsonPropertyName("given_name")]
        public string? GivenName { get; set; }
        [JsonPropertyName("family_name")]
        public string? FamilyName { get; set; }
    }

    private sealed class FacebookUserInfo
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }
        [JsonPropertyName("email")]
        public string? Email { get; set; }
        [JsonPropertyName("first_name")]
        public string? FirstName { get; set; }
        [JsonPropertyName("last_name")]
        public string? LastName { get; set; }
        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }

    private sealed class FacebookDebugTokenResponse
    {
        [JsonPropertyName("data")]
        public FacebookDebugTokenData? Data { get; set; }
    }

    private sealed class FacebookDebugTokenData
    {
        [JsonPropertyName("app_id")]
        public string? AppId { get; set; }
        [JsonPropertyName("user_id")]
        public string? UserId { get; set; }
        [JsonPropertyName("is_valid")]
        public bool IsValid { get; set; }
    }
}
