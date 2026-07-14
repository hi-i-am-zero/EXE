namespace AutoWork.Application.DTOs.Campaigns;

public class CampaignDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string GoalName { get; set; } = string.Empty;
    public string PromotionTypeName { get; set; } = string.Empty;
    public decimal? DiscountPercent { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal? MinOrderAmount { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public int NumberOfPosts { get; set; }
    public int Status { get; set; }
    public DateTime CreatedAt { get; set; }
}
