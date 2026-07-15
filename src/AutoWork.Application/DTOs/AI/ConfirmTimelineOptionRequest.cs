namespace AutoWork.Application.DTOs.AI;

/// <summary>Gửi lại đúng nội dung của phương án đã chọn (đã trả về ở bước GenerateTimelineOptions)
/// để backend thật sự tạo Timeline + Post + PostContent + Hashtag.</summary>
public class ConfirmTimelineOptionRequest
{
    public Guid CampaignId { get; set; }
    public Guid TemplateTypeId { get; set; }
    public string TimelineName { get; set; } = string.Empty;
    public List<DraftPostItemDto> Posts { get; set; } = [];
}
