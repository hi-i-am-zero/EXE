namespace AutoWork.Application.DTOs.Facebook;

public class PublishFacebookPostDto
{
    public Guid FacebookPageId { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Link { get; set; }
    public Guid? MediaFileId { get; set; }
    public DateTime? ScheduledAt { get; set; }
}
