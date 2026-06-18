using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Domain.Entities;
using AutoWork.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace AutoWork.Persistence.Repositories;

public class CreditRepository : Repository<Credit>, ICreditRepository
{
    public CreditRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Credit?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
        await DbSet
            .Include(c => c.Transactions)
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

    public async Task<IReadOnlyList<CreditTransaction>> GetTransactionsByUserIdAsync(
        Guid userId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default) =>
        await Context.CreditTransactions
            .AsNoTracking()
            .Where(ct => ct.UserId == userId)
            .OrderByDescending(ct => ct.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public async Task<int> CountTransactionsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
        await Context.CreditTransactions.CountAsync(ct => ct.UserId == userId, cancellationToken);

    public async Task AddTransactionAsync(CreditTransaction transaction, CancellationToken cancellationToken = default) =>
        await Context.CreditTransactions.AddAsync(transaction, cancellationToken);
}
