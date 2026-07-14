using AutoWork.Domain.Common;

namespace AutoWork.Domain.Entities;

/// <summary>Lookup: kiểu mẫu timeline (Classic Campaign / Roadmap / Calendar).</summary>
public class TimelineTemplateType : BaseEntity
{
    public string TypeName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public ICollection<Timeline> Timelines { get; set; } = new List<Timeline>();
}
