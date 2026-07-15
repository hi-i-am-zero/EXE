using AutoWork.Domain.Common;

namespace AutoWork.Domain.Entities;

/// <summary>Lookup: loại ưu đãi (Giảm %, Freeship, Mua X tặng Y...).</summary>
public class PromotionType : BaseEntity
{
    public string TypeName { get; set; } = string.Empty;

    public ICollection<Campaign> Campaigns { get; set; } = new List<Campaign>();
}
