namespace AutoWork.Application.DTOs.Plans;

public class SubscriptionDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid PlanId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public int Status { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? CancelledAt { get; set; }
    public bool AutoRenew { get; set; }
}
