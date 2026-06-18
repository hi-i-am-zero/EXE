using AutoWork.Domain.Common;

namespace AutoWork.Domain.Entities;

/// <summary>Immutable record of a credit balance change.</summary>
public class CreditTransaction : BaseEntity
{
    public Guid CreditId { get; set; }

    public Guid UserId { get; set; }

    public int Type { get; set; }

    public int Amount { get; set; }

    public int BalanceAfter { get; set; }

    public string? Description { get; set; }

    public string? ReferenceType { get; set; }

    public Guid? ReferenceId { get; set; }

    public Credit Credit { get; set; } = null!;

    public User User { get; set; } = null!;
}
