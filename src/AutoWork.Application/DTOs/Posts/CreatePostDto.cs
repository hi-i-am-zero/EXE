namespace AutoWork.Application.DTOs.Posts;

public class CreatePostDto
{
    public Guid ProjectId { get; set; }
    public Guid ChannelAccountId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int ContentType { get; set; } = 1;
    public Guid? MediaFileId { get; set; }
}
