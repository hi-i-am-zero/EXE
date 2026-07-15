using AutoWork.Domain.Common;

namespace AutoWork.Domain.Entities;

/// <summary>Facebook page linked to a user account.</summary>
public class FacebookPage : BaseEntity
{
    public Guid FacebookAccountId { get; set; }

    public string PageId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Category { get; set; }

    public string? ProfilePictureUrl { get; set; }

    public bool IsConnected { get; set; } = true;

    public DateTime? LastSyncedAt { get; set; }

    public FacebookAccount FacebookAccount { get; set; } = null!;
}
