using Microsoft.Extensions.Caching.Memory;

namespace AutoWork.Infrastructure.Services;

/// <summary>Volatile OAuth token cache for third-party integrations.</summary>
public class IntegrationTokenStore
{
    private readonly IMemoryCache _cache;
    private static readonly TimeSpan DefaultTtl = TimeSpan.FromDays(30);

    public IntegrationTokenStore(IMemoryCache cache)
    {
        _cache = cache;
    }

    public void Set(string key, string token, DateTime? expiresAt = null)
    {
        var ttl = expiresAt.HasValue
            ? expiresAt.Value - DateTime.UtcNow
            : DefaultTtl;

        if (ttl <= TimeSpan.Zero)
        {
            ttl = TimeSpan.FromHours(1);
        }

        _cache.Set(key, token, ttl);
    }

    public string? Get(string key) =>
        _cache.TryGetValue(key, out string? token) ? token : null;

    public void Remove(string key) =>
        _cache.Remove(key);

    public static string FacebookUserTokenKey(Guid accountId) => $"facebook:account:{accountId}:token";

    public static string FacebookPageTokenKey(Guid pageId) => $"facebook:page:{pageId}:token";

    public static string ZaloAccountTokenKey(Guid accountId) => $"zalo:account:{accountId}:token";
}
