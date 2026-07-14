namespace AutoWork.Application.DTOs.Payments;

public class InvoiceDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "VND";
    public int Status { get; set; }
    public string? Description { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
