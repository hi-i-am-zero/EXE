using AutoWork.Domain.Common;

namespace AutoWork.Domain.Entities;

/// <summary>Lookup: CTA phrases hay dùng (Chốt đơn ngay, Inbox tư vấn...).</summary>
public class CtaTemplate : BaseEntity
{
    public string CtaText { get; set; } = string.Empty;

    public ICollection<BrandCta> BrandCtas { get; set; } = new List<BrandCta>();
}
