using AutoWork.Shared.Enums;

namespace AutoWork.Application.DTOs.AI;

/// <summary>Bước 2 của luồng tạo Chiến dịch tự động: upload ảnh sản phẩm, AI sinh mô tả chung,
/// lưu vào Campaign.ProductDescription để dùng làm ngữ cảnh sinh timeline ở bước 3.</summary>
public class GenerateCampaignProductDescriptionRequest
{
    public Guid CampaignId { get; set; }
    public List<Guid> MediaFileIds { get; set; } = [];
    public AiProvider Provider { get; set; } = AiProvider.Claude;
}
