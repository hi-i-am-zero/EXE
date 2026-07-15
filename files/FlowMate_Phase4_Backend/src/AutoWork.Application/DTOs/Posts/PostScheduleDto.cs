namespace AutoWork.Application.DTOs.Posts;

public class PostScheduleDto
{
    public Guid Id { get; set; }
    public Guid PostId { get; set; }
    public DateTime ScheduledAt { get; set; }
    public int Status { get; set; }
    public DateTime? ExecutedAt { get; set; }
    public string? FailureReason { get; set; }
    public int RetryCount { get; set; }
}
