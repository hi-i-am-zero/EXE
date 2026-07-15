using AutoWork.Domain.Common;

namespace AutoWork.Domain.Entities;

/// <summary>Supported publishing channel type (e.g. Facebook, WordPress).</summary>
public class Channel : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? IconUrl { get; set; }

    public bool IsActive { get; set; } = true;

    public int SortOrder { get; set; }

    public ICollection<ChannelAccount> ChannelAccounts { get; set; } = new List<ChannelAccount>();
}
