namespace AutoWork.Infrastructure.Settings;

public class FacebookSettings
{
    public const string SectionName = "FacebookSettings";

    public string AppId { get; set; } = string.Empty;

    public string AppSecret { get; set; } = string.Empty;

    public string GraphApiVersion { get; set; } = "v21.0";

    public string GraphApiBaseUrl { get; set; } = "https://graph.facebook.com";

    public string[] Scopes { get; set; } =
    [
        "pages_show_list",
        "pages_read_engagement",
        "pages_manage_posts",
        "pages_manage_metadata"
    ];
}
