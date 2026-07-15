namespace AutoWork.Domain.Enums;

/// <summary>Payment state of an invoice.</summary>
public enum InvoiceStatus
{
    Draft = 0,
    Pending = 1,
    Paid = 2,
    Overdue = 3,
    Cancelled = 4,
    Refunded = 5
}
