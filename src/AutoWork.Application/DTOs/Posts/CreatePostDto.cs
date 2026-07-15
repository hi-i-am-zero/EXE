namespace AutoWork.Application.DTOs.Posts;

public class CreatePostDto
{
    public Guid ProjectId { get; set; }

    /// <summary>Legacy: 1 nền tảng duy nhất. Giữ lại để tương thích API cũ — nếu ChannelAccountIds
    /// rỗng, hệ thống sẽ dùng field này như 1 phần tử duy nhất.</summary>
    public Guid ChannelAccountId { get; set; }

    /// <summary>FlowMate v2: chọn nhiều nền tảng cùng lúc (VD: Facebook + Instagram), khớp UI "Tạo bài viết đơn".</summary>
    public List<Guid> ChannelAccountIds { get; set; } = [];

    public Guid? TimelineId { get; set; }
    public Guid? VoiceSampleId { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int ContentType { get; set; } = 1;
    public Guid? MediaFileId { get; set; }

    /// <summary>Hashtag nhập tự do (VD: "FlowMate", "VayHoaNhi") — hệ thống tự tìm-hoặc-tạo trong bảng Hashtags dùng chung.</summary>
    public List<string> Hashtags { get; set; } = [];
}
