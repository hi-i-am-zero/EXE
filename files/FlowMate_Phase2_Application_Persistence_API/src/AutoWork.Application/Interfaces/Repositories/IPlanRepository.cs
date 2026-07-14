using AutoWork.Domain.Entities;

namespace AutoWork.Application.Interfaces.Repositories;

public interface IPlanRepository : IRepository<Plan>
{
    Task<IReadOnlyList<Plan>> GetActivePlansAsync(CancellationToken cancellationToken = default);
    Task<Plan?> GetByIdWithSubscriptionsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Plan?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<Subscription?> GetActiveSubscriptionAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Subscription> AddSubscriptionAsync(Subscription subscription, CancellationToken cancellationToken = default);
    Task UpdateSubscriptionAsync(Subscription subscription, CancellationToken cancellationToken = default);
}
