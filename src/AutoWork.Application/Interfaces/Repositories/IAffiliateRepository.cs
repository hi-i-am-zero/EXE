using AutoWork.Domain.Entities;

namespace AutoWork.Application.Interfaces.Repositories;

public interface IAffiliateRepository : IRepository<Affiliate>
{
    Task<Affiliate?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Affiliate?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AffiliateCommission>> GetCommissionsByAffiliateIdAsync(Guid affiliateId, CancellationToken cancellationToken = default);
    Task AddCommissionAsync(AffiliateCommission commission, CancellationToken cancellationToken = default);
}
