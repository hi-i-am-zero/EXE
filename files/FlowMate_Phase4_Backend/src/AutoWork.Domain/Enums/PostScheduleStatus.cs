namespace AutoWork.Domain.Enums;

/// <summary>State of a scheduled post execution.</summary>
public enum PostScheduleStatus
{
    Pending = 0,
    Processing = 1,
    Completed = 2,
    Failed = 3,
    Cancelled = 4
}
