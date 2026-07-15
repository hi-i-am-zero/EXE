namespace AutoWork.Application.DTOs.Timelines;

public class TimelineDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid? CampaignId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string TemplateTypeName { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public int Status { get; set; }
    public int PostCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
