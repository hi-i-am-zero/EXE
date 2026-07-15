using AutoWork.Application.DTOs.BrandMemory;

namespace AutoWork.Application.DTOs.Campaigns;

public class CampaignLookupOptionsDto
{
    public List<LookupItemDto> Goals { get; set; } = [];
    public List<LookupItemDto> PromotionTypes { get; set; } = [];
    public List<LookupItemDto> TimelineTemplateTypes { get; set; } = [];
}
