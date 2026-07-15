using AutoWork.Shared.Enums;

namespace AutoWork.Application.DTOs.AI;

public class GenerateTimelinePostsRequest
{
    public Guid TimelineId { get; set; }

    public AiProvider Provider { get; set; } = AiProvider.Claude;

    /// <summary>Số bài viết muốn AI tạo. Nếu để trống: lấy theo Campaign.NumberOfPosts (nếu Timeline
    /// có gắn Campaign), hoặc mặc định 5.</summary>
    public int? PostCount { get; set; }
}
