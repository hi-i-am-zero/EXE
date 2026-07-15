using AutoWork.Domain.Common;

namespace AutoWork.Domain.Entities;

/// <summary>User credit balance aggregate.</summary>
public class Credit : BaseEntity
{
    public Guid UserId { get; set; }

    public int Balance { get; set; }

    public int TotalEarned { get; set; }

    public int TotalUsed { get; set; }

    public User User { get; set; } = null!;

    public ICollection<CreditTransaction> Transactions { get; set; } = new List<CreditTransaction>();
}
