namespace AutoWork.Infrastructure.Settings;

public class ZaloSettings
{
    public const string SectionName = "ZaloSettings";

    public string AppId { get; set; } = string.Empty;

    public string AppSecret { get; set; } = string.Empty;

    public string OaApiBaseUrl { get; set; } = "https://openapi.zalo.me/v3.0/oa";

    public string OAuthBaseUrl { get; set; } = "https://oauth.zaloapp.com/v4/oa";

    public string[] Scopes { get; set; } = ["manage_page"];
}
