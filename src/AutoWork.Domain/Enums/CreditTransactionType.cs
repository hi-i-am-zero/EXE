namespace AutoWork.Domain.Enums;

/// <summary>Type of credit balance change.</summary>
public enum CreditTransactionType
{
    Purchase = 0,
    SubscriptionGrant = 1,
    Usage = 2,
    Refund = 3,
    Bonus = 4,
    Adjustment = 5
}
