namespace AutoWork.Application.DTOs.Credits;

public class CreditTransactionDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public int Type { get; set; }
    public int Amount { get; set; }
    public int BalanceAfter { get; set; }
    public string? Description { get; set; }
    public string? ReferenceType { get; set; }
    public Guid? ReferenceId { get; set; }
    public DateTime CreatedAt { get; set; }
}
