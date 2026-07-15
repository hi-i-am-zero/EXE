namespace AutoWork.Application.DTOs.AI;

/// <summary>1 bài viết nháp do AI đề xuất — chưa lưu DB (dùng trong bước "3 lựa chọn timeline",
/// chỉ khi user CHỌN 1 phương án thì mới thật sự tạo Post/PostContent trong ConfirmTimelineOptionCommand).</summary>
public class DraftPostItemDto
{
    public int DayOffset { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public List<string> Hashtags { get; set; } = [];
}
