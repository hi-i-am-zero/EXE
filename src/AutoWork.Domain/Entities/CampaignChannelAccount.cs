using AutoWork.Domain.Common;

namespace AutoWork.Domain.Entities;

/// <summary>Join entity: Campaign &lt;-&gt; ChannelAccount (1 chiến dịch chạy trên nhiều tài khoản kênh cụ thể).</summary>
public class CampaignChannelAccount : BaseEntity
{
    public Guid CampaignId { get; set; }

    public Guid ChannelAccountId { get; set; }

    public Campaign Campaign { get; set; } = null!;

    public ChannelAccount ChannelAccount { get; set; } = null!;
}
