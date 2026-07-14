using AutoWork.Domain.Entities;

namespace AutoWork.Application.Interfaces.Repositories;

public interface ICreditRepository : IRepository<Credit>
{
    Task<Credit?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CreditTransaction>> GetTransactionsByUserIdAsync(Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountTransactionsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddTransactionAsync(CreditTransaction transaction, CancellationToken cancellationToken = default);
}
