using AutoWork.Domain.Common;

namespace AutoWork.Domain.Entities;

/// <summary>Connected channel account within a project.</summary>
public class ChannelAccount : BaseEntity
{
    public Guid ProjectId { get; set; }

    public Guid ChannelId { get; set; }

    public Guid UserId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? ExternalId { get; set; }

    public string? ProfileUrl { get; set; }

    public string? AvatarUrl { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime? LastSyncedAt { get; set; }

    public Project Project { get; set; } = null!;

    public Channel Channel { get; set; } = null!;

    public User User { get; set; } = null!;

    public ICollection<Post> Posts { get; set; } = new List<Post>();
}
