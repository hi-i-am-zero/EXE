namespace AutoWork.Application.DTOs.BrandMemory;

/// <summary>Request dùng chung cho việc chọn lại 1 tập lookup (Styles/Keywords/CTA/Hashtags) của Brand Memory.</summary>
public class SetBrandTagsDto
{
    public Guid ProjectId { get; set; }

    public List<Guid> SelectedIds { get; set; } = [];
}
