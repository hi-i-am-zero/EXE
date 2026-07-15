namespace AutoWork.Application.DTOs.AI;

/// <summary>1 trong 3 phương án timeline do AI đề xuất — gắn với 1 kiểu mẫu (Classic/Roadmap/Calendar).</summary>
public class TimelineOptionDto
{
    public Guid TemplateTypeId { get; set; }
    public string TemplateTypeName { get; set; } = string.Empty;

    /// <summary>Mô tả ngắn về chiến lược của phương án này (VD: "Ngày 1 teaser, giữa tháng ra mắt, cuối tháng flash sale").</summary>
    public string StrategyDescription { get; set; } = string.Empty;

    public List<DraftPostItemDto> Posts { get; set; } = [];
}
