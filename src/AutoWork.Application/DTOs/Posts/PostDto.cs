namespace AutoWork.Application.DTOs.Posts;

public class PostDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public Guid ChannelAccountId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Status { get; set; }
    public string? ExternalPostId { get; set; }
    public string? PublishedUrl { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public PostScheduleDto? Schedule { get; set; }
}
