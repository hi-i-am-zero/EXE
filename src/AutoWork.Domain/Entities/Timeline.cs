using AutoWork.Domain.Common;

namespace AutoWork.Domain.Entities;

/// <summary>Lịch nội dung (Classic Campaign / Roadmap / Calendar), có thể gắn hoặc không gắn 1 Campaign.</summary>
public class Timeline : BaseEntity
{
    public Guid ProjectId { get; set; }

    /// <summary>NULL nếu timeline độc lập, không gắn chiến dịch.</summary>
    public Guid? CampaignId { get; set; }

    public string Name { get; set; } = string.Empty;

    public Guid TemplateTypeId { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    /// <summary>enum AutoWork.Domain.Enums.TimelineStatus.</summary>
    public int Status { get; set; }

    public Project Project { get; set; } = null!;

    public Campaign? Campaign { get; set; }

    public TimelineTemplateType TemplateType { get; set; } = null!;

    public ICollection<Post> Posts { get; set; } = new List<Post>();
}
