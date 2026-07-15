namespace AutoWork.Application.DTOs.Posts;

public class PublishPostResultDto
{
    public Guid ChannelAccountId { get; set; }
    public string ChannelCode { get; set; } = string.Empty;
    public string ChannelAccountName { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? ExternalPostId { get; set; }
    public string? PublishedUrl { get; set; }
    public string? ErrorMessage { get; set; }
}

public class PublishPostResponse
{
    public Guid PostId { get; set; }
    public int PostStatus { get; set; }
    public List<PublishPostResultDto> Results { get; set; } = [];
}
