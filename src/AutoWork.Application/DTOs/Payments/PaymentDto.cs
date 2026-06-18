using AutoWork.Shared.Enums;

namespace AutoWork.Application.DTOs.Payments;

public class PaymentDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? InvoiceId { get; set; }
    public Guid? PlanId { get; set; }
    public PaymentProvider Provider { get; set; }
    public int Status { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "VND";
    public string TransactionId { get; set; } = string.Empty;

    public string? PaymentUrl { get; set; }

    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
