using AutoWork.Domain.Common;

namespace AutoWork.Domain.Entities;

/// <summary>Reusable AI prompt template.</summary>
public class AiPrompt : BaseEntity
{
    public Guid? UserId { get; set; }

    public Guid? ProjectId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Template { get; set; } = string.Empty;

    public string? Category { get; set; }

    public bool IsSystem { get; set; }

    public User? User { get; set; }

    public Project? Project { get; set; }

    public ICollection<AiGeneratedContent> GeneratedContents { get; set; } = new List<AiGeneratedContent>();
}
