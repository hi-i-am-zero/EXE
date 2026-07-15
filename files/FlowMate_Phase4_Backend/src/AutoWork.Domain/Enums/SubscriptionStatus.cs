namespace AutoWork.Domain.Enums;

/// <summary>Lifecycle state of a user subscription.</summary>
public enum SubscriptionStatus
{
    Pending = 0,
    Active = 1,
    Cancelled = 2,
    Expired = 3,
    Suspended = 4
}
