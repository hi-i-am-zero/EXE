using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Domain.Entities;
using AutoWork.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace AutoWork.Persistence.Repositories;

public class AuditLogRepository : Repository<AuditLog>, IAuditLogRepository
{
    public AuditLogRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<AuditLog>> GetRecentByUserIdAsync(
        Guid userId,
        int count,
        CancellationToken cancellationToken = default) =>
        await DbSet
            .AsNoTracking()
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .Take(count)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<AuditLog>> GetRecentAsync(
        int count,
        CancellationToken cancellationToken = default) =>
        await DbSet
            .AsNoTracking()
            .OrderByDescending(a => a.CreatedAt)
            .Take(count)
            .ToListAsync(cancellationToken);
}
