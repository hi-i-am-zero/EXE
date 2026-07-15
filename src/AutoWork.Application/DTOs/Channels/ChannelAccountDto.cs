namespace AutoWork.Application.DTOs.Channels;

public class ChannelAccountDto
{
    public Guid Id { get; set; }
    public Guid ChannelId { get; set; }
    public string ChannelCode { get; set; } = string.Empty;
    public string ChannelName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ProfileUrl { get; set; }
    public string? AvatarUrl { get; set; }
    public bool IsActive { get; set; }

    /// <summary>Đã liên kết API thật (có AccessToken) hay chỉ mới tạo thủ công — để UI hiển thị đúng trạng thái.</summary>
    public bool HasApiConnection { get; set; }
}
