using AutoWork.Shared.Enums;

namespace AutoWork.Application.DTOs.Payments;

public class CreatePaymentRequest
{
    public Guid PlanId { get; set; }
    public PaymentProvider Provider { get; set; }
    public string ReturnUrl { get; set; } = string.Empty;
    public string CancelUrl { get; set; } = string.Empty;
}
