using AutoWork.Application.DTOs.Posts;
using MediatR;

namespace AutoWork.Application.Features.Posts.Commands;

/// <summary>Đăng 1 bài viết lên TẤT CẢ nền tảng đã chọn (PostChannelAccounts). Hiện tại chỉ Facebook
/// có API thật (đã gọi Graph API để đăng); các nền tảng khác (Instagram/TikTok/Zalo/Website) trả về
/// lỗi rõ ràng "chưa hỗ trợ" thay vì giả vờ thành công — đúng theo yêu cầu để trống phần API còn lại.</summary>
public class PublishPostCommand : IRequest<PublishPostResponse>
{
    public Guid PostId { get; set; }
}
