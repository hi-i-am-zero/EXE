using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Domain.Entities;
using AutoWork.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace AutoWork.Persistence.Repositories;

public class AffiliateRepository : Repository<Affiliate>, IAffiliateRepository
{
    public AffiliateRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Affiliate?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
        await DbSet
            .Include(a => a.Links)
            .Include(a => a.Commissions)
            .FirstOrDefaultAsync(a => a.UserId == userId, cancellationToken);

    public async Task<Affiliate?> GetByCodeAsync(string code, CancellationToken cancellationToken = default) =>
        await DbSet.FirstOrDefaultAsync(a => a.Code == code, cancellationToken);

    public async Task<IReadOnlyList<AffiliateCommission>> GetCommissionsByAffiliateIdAsync(
        Guid affiliateId,
        CancellationToken cancellationToken = default) =>
        await Context.AffiliateCommissions
            .AsNoTracking()
            .Include(ac => ac.ReferredUser)
            .Where(ac => ac.AffiliateId == affiliateId)
            .OrderByDescending(ac => ac.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task AddCommissionAsync(
        AffiliateCommission commission,
        CancellationToken cancellationToken = default) =>
        await Context.AffiliateCommissions.AddAsync(commission, cancellationToken);
}
