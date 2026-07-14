using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Domain.Entities;
using AutoWork.Domain.Enums;
using AutoWork.Persistence.Context;
using AutoWork.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace AutoWork.Persistence.Repositories;

public class PlanRepository : Repository<Plan>, IPlanRepository
{
    public PlanRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Plan>> GetActivePlansAsync(CancellationToken cancellationToken = default) =>
        await DbSet
            .AsNoTracking()
            .Where(p => p.IsActive)
            .OrderBy(p => p.SortOrder)
            .ToListAsync(cancellationToken);

    public async Task<Plan?> GetByIdWithSubscriptionsAsync(Guid id, CancellationToken cancellationToken = default) =>
        await DbSet
            .Include(p => p.Subscriptions)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<Plan?> GetByCodeAsync(string code, CancellationToken cancellationToken = default) =>
        await DbSet.FirstOrDefaultAsync(p => p.Code == code && p.IsActive, cancellationToken);

    public async Task<Subscription?> GetActiveSubscriptionAsync(Guid userId, CancellationToken cancellationToken = default) =>
        await Context.Subscriptions
            .Include(s => s.Plan)
            .Where(s => s.UserId == userId && s.Status == (int)Domain.Enums.SubscriptionStatus.Active && s.EndDate >= DateTime.UtcNow)
            .OrderByDescending(s => s.EndDate)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<Subscription> AddSubscriptionAsync(Subscription subscription, CancellationToken cancellationToken = default)
    {
        await Context.Subscriptions.AddAsync(subscription, cancellationToken);
        return subscription;
    }

    public Task UpdateSubscriptionAsync(Subscription subscription, CancellationToken cancellationToken = default)
    {
        Context.Subscriptions.Update(subscription);
        return Task.CompletedTask;
    }
}
