using AutoWork.Domain.Entities;

namespace AutoWork.Application.Interfaces.Repositories;

public interface IAuditLogRepository : IRepository<AuditLog>
{
    Task<IReadOnlyList<AuditLog>> GetRecentByUserIdAsync(Guid userId, int count, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AuditLog>> GetRecentAsync(int count, CancellationToken cancellationToken = default);
}
