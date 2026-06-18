using AutoWork.Domain.Common;

namespace AutoWork.Domain.Entities;

/// <summary>Payment transaction against an invoice.</summary>
public class Payment : BaseEntity
{
    public Guid InvoiceId { get; set; }

    public Guid UserId { get; set; }

    public decimal Amount { get; set; }

    public int PaymentMethod { get; set; }

    public string? TransactionId { get; set; }

    public int Status { get; set; }

    public DateTime? PaidAt { get; set; }

    public string? FailureReason { get; set; }

    public Invoice Invoice { get; set; } = null!;

    public User User { get; set; } = null!;
}
