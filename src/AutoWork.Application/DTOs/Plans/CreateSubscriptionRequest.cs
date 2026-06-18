namespace AutoWork.Application.DTOs.Plans;

public class CreateSubscriptionRequest
{
    public Guid PlanId { get; set; }
    public bool AutoRenew { get; set; } = true;
}
