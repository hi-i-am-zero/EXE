namespace AutoWork.Application.DTOs.Channels;

public class CreateChannelAccountDto
{
    public Guid ProjectId { get; set; }
    public Guid ChannelId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ProfileUrl { get; set; }
}
