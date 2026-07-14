namespace AutoWork.Application.DTOs.Schedules;

public class ScheduleDto
{
    public Guid Id { get; set; }
    public Guid PostId { get; set; }
    public DateTime ScheduledAt { get; set; }
    public int RecurrenceType { get; set; }
    public bool IsProcessed { get; set; }
}

public class CreateScheduleRequest
{
    public Guid PostId { get; set; }
    public DateTime ScheduledAt { get; set; }
    public int RecurrenceType { get; set; }
}

public class UpdateScheduleRequest
{
    public DateTime ScheduledAt { get; set; }
    public int RecurrenceType { get; set; }
}
