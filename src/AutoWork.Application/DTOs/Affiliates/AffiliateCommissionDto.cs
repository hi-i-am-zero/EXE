namespace AutoWork.Application.DTOs.Affiliates;

public class AffiliateCommissionDto
{
    public Guid Id { get; set; }
    public Guid AffiliateId { get; set; }
    public Guid ReferredUserId { get; set; }
    public string ReferredUserEmail { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int Status { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
