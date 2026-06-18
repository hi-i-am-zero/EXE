using AutoWork.Domain.Common;

namespace AutoWork.Domain.Entities;

/// <summary>Application-wide configuration setting.</summary>
public class Setting : BaseEntity
{
    public string Key { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;

    public string? Category { get; set; }

    public string? Description { get; set; }

    public bool IsPublic { get; set; }
}
