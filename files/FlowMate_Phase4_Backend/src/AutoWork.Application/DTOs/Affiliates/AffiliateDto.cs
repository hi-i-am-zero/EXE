namespace AutoWork.Application.DTOs.Affiliates;

public class AffiliateDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Code { get; set; } = string.Empty;
    public decimal CommissionRate { get; set; }
    public decimal TotalEarnings { get; set; }
    public decimal PendingEarnings { get; set; }
    public bool IsActive { get; set; }
}
