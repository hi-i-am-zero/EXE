using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Domain.Entities;
using AutoWork.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace AutoWork.Persistence.Repositories;

public class PendingRegistrationRepository : Repository<PendingRegistration>, IPendingRegistrationRepository
{
    public PendingRegistrationRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<PendingRegistration?> GetByTokenAsync(string token, CancellationToken cancellationToken = default) =>
        await DbSet.FirstOrDefaultAsync(p => p.Token == token, cancellationToken);

    public async Task<PendingRegistration?> GetActiveByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        await DbSet.FirstOrDefaultAsync(
            p => p.Email == email && p.UsedAt == null && p.ExpiresAt > DateTime.UtcNow,
            cancellationToken);

    public async Task<PendingRegistration?> GetActiveByPhoneAsync(string phone, CancellationToken cancellationToken = default) =>
        await DbSet.FirstOrDefaultAsync(
            p => p.Phone == phone && p.UsedAt == null && p.ExpiresAt > DateTime.UtcNow,
            cancellationToken);

    public async Task RemoveActiveByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var rows = await DbSet
            .Where(p => p.Email == email && p.UsedAt == null)
            .ToListAsync(cancellationToken);

        DbSet.RemoveRange(rows);
    }
}
