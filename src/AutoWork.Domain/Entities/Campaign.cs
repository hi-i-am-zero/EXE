using AutoWork.Domain.Common;

namespace AutoWork.Domain.Entities;

/// <summary>Chiến dịch tự động: mục tiêu, ưu đãi, khung thời gian, số bài đăng.</summary>
public class Campaign : BaseEntity
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

    /// <summary>Mô tả sản phẩm chung do AI sinh ra từ ảnh upload ở bước 2 của luồng tạo chiến dịch —
    /// dùng làm ngữ cảnh khi AI sinh 3 phương án timeline ở bước 3.</summary>
    public string? ProductDescription { get; set; }

    /// <summary>enum AutoWork.Domain.Enums.CampaignStatus.</summary>
    public int Status { get; set; }

    public Project Project { get; set; } = null!;

    public CampaignGoal Goal { get; set; } = null!;

    public PromotionType PromotionType { get; set; } = null!;

    public ICollection<CampaignChannelAccount> CampaignChannelAccounts { get; set; } = new List<CampaignChannelAccount>();

    public ICollection<Timeline> Timelines { get; set; } = new List<Timeline>();
}
