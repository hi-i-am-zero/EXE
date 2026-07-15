using AutoWork.Application.DTOs.AI;
using AutoWork.Application.DTOs.Timelines;

namespace AutoWork.Application.Interfaces.Services;

/// <summary>
/// AI sinh 3 phương án timeline (mỗi phương án gắn 1 kiểu mẫu Classic/Roadmap/Calendar) dựa trên
/// mục tiêu Campaign + mô tả sản phẩm (Campaign.ProductDescription) + Brand Memory của workspace.
/// Đúng luồng: GenerateTimelineOptionsAsync KHÔNG lưu DB (chỉ trả về để user xem & chọn) —
/// chỉ khi ConfirmTimelineOptionAsync được gọi (user đã chọn 1 phương án) mới thật sự tạo
/// Timeline + Post + PostContent + Hashtag.
/// </summary>
public interface ITimelineContentService
{
    Task<GenerateTimelineOptionsResponse> GenerateTimelineOptionsAsync(
        Guid userId, GenerateTimelineOptionsRequest request, CancellationToken cancellationToken = default);

    Task<TimelineDto> ConfirmTimelineOptionAsync(
        Guid userId, ConfirmTimelineOptionRequest request, CancellationToken cancellationToken = default);
}
