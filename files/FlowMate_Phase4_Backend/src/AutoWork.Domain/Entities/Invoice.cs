using AutoWork.Domain.Common;

namespace AutoWork.Domain.Entities;

/// <summary>Billing invoice for a user.</summary>
public class Invoice : BaseEntity
{
    public Guid UserId { get; set; }

    public Guid? SubscriptionId { get; set; }

    public string InvoiceNumber { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public decimal Tax { get; set; }

    public decimal Total { get; set; }

    public int Status { get; set; }

    public DateTime DueDate { get; set; }

    public DateTime? PaidAt { get; set; }

    public string? Notes { get; set; }

    public User User { get; set; } = null!;

    public Subscription? Subscription { get; set; }

    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
