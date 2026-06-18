using AutoWork.Domain.Common;

namespace AutoWork.Domain.Entities;

/// <summary>Marketing workspace owned by a user.</summary>
public class Project : BaseEntity
{
    public Guid UserId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? LogoUrl { get; set; }

    public bool IsActive { get; set; } = true;

    public User User { get; set; } = null!;

    public ICollection<ChannelAccount> ChannelAccounts { get; set; } = new List<ChannelAccount>();

    public ICollection<Post> Posts { get; set; } = new List<Post>();

    public ICollection<AiPrompt> AiPrompts { get; set; } = new List<AiPrompt>();

    public ICollection<AiGeneratedContent> AiGeneratedContents { get; set; } = new List<AiGeneratedContent>();

    public ICollection<MediaFile> MediaFiles { get; set; } = new List<MediaFile>();
}
