namespace AutoWork.Application.DTOs.WordPress;

public class WordPressSiteDto
{
    public Guid Id { get; set; }
    public string SiteName { get; set; } = string.Empty;
    public string SiteUrl { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;

    public string ApplicationPassword { get; set; } = string.Empty;

    public bool IsWooCommerce { get; set; }
    public bool IsActive { get; set; }
}
