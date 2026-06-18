namespace AutoWork.Domain.Enums;

/// <summary>Processing state of AI-generated content.</summary>
public enum AiContentStatus
{
    Pending = 0,
    Processing = 1,
    Completed = 2,
    Failed = 3
}
