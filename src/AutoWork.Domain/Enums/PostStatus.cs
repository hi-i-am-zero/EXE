namespace AutoWork.Domain.Enums;

/// <summary>Overall publishing state of a post.</summary>
public enum PostStatus
{
    Draft = 0,
    Scheduled = 1,
    Publishing = 2,
    Published = 3,
    Failed = 4,
    Cancelled = 5
}
