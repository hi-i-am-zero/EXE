using AutoWork.Application.DTOs.AI;

namespace AutoWork.Application.Interfaces.Services;

/// <summary>AI sinh bộ bài viết nháp (title/content/hashtag) rải đều theo khung thời gian của 1
/// Timeline, dựa vào mục tiêu Campaign (nếu có) và Brand Memory (giọng văn, phong cách) của workspace.
/// Tách riêng khỏi IAiContentService vì input/output là 1 MẢNG bài viết, không phải 1 nội dung đơn lẻ.</summary>
public interface ITimelineContentService
{
    Task<GenerateTimelinePostsResponse> GenerateTimelinePostsAsync(
        Guid userId, GenerateTimelinePostsRequest request, CancellationToken cancellationToken = default);
}
