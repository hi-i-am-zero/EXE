using AutoWork.Domain.Common;

namespace AutoWork.Domain.Entities;

/// <summary>Granular permission definition.</summary>
public class Permission : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Module { get; set; }
}
