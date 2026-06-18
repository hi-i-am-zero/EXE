namespace AutoWork.Domain.Enums;

/// <summary>State of a payment transaction.</summary>
public enum PaymentStatus
{
    Pending = 0,
    Processing = 1,
    Completed = 2,
    Failed = 3,
    Refunded = 4
}
