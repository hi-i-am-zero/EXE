namespace AutoWork.Application.DTOs.Timelines;

public class CreateTimelineDto
{
    public Guid ProjectId { get; set; }

    /// <summary>NULL nếu timeline độc lập, không gắn chiến dịch.</summary>
    public Guid? CampaignId { get; set; }

    public string Name { get; set; } = string.Empty;
    public Guid TemplateTypeId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
}
