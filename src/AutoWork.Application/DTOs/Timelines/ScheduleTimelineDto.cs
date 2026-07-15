namespace AutoWork.Application.DTOs.Timelines;

public class ScheduleTimelineRequest
{
    public Guid TimelineId { get; set; }

    /// <summary>"lịch đăng bài ở đây sẽ có thể điều chỉnh bài đầu tiên thành đăng ngay lập tức" —
    /// nếu true, bài đầu tiên (DayNumber=1) được đăng thật ngay lập tức thay vì chỉ lên lịch.</summary>
    public bool PublishFirstImmediately { get; set; }
}

public class ScheduleTimelineResponse
{
    public Guid TimelineId { get; set; }
    public int ScheduledCount { get; set; }
    public bool FirstPublishedImmediately { get; set; }
    public string? FirstPublishError { get; set; }
}
