namespace AutoWork.Application.DTOs.Campaigns;

public class CreateCampaignDto
{
    public Guid ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid GoalId { get; set; }
    public Guid PromotionTypeId { get; set; }
    public decimal? DiscountPercent { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal? MinOrderAmount { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public int NumberOfPosts { get; set; } = 1;

    /// <summary>ChannelAccount cụ thể mà chiến dịch chạy trên đó (VD: Fanpage Facebook đã kết nối).</summary>
    public List<Guid> ChannelAccountIds { get; set; } = [];
}
