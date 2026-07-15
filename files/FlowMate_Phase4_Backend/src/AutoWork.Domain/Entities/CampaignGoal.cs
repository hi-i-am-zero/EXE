using AutoWork.Domain.Common;

namespace AutoWork.Domain.Entities;

/// <summary>Lookup: mục tiêu chiến dịch (Ra mắt sản phẩm mới, Xả hàng tồn kho...).</summary>
public class CampaignGoal : BaseEntity
{
    public string GoalName { get; set; } = string.Empty;

    public ICollection<Campaign> Campaigns { get; set; } = new List<Campaign>();
}
