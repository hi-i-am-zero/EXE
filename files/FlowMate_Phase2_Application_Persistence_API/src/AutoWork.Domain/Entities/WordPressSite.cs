using AutoWork.Domain.Common;

namespace AutoWork.Domain.Entities;

/// <summary>Connected WordPress site.</summary>
public class WordPressSite : BaseEntity
{
    public Guid UserId { get; set; }

    public string SiteUrl { get; set; } = string.Empty;

    public string SiteName { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public string ApplicationPassword { get; set; } = string.Empty;

    public bool IsWooCommerce { get; set; }

    public bool IsConnected { get; set; } = true;

    public DateTime? LastSyncedAt { get; set; }

    public User User { get; set; } = null!;

    public ICollection<WordPressPost> Posts { get; set; } = new List<WordPressPost>();
}
