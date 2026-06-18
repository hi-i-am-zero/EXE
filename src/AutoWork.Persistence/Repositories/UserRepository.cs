using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Domain.Entities;
using AutoWork.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace AutoWork.Persistence.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        await DbSet.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    public async Task<User?> GetByIdWithRolesAsync(Guid id, CancellationToken cancellationToken = default) =>
        await DbSet
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default) =>
        await DbSet.AnyAsync(u => u.Email == email, cancellationToken);

    public async Task<User?> GetByReferralCodeAsync(string referralCode, CancellationToken cancellationToken = default) =>
        await DbSet.FirstOrDefaultAsync(u => u.ReferralCode == referralCode, cancellationToken);

    public async Task<IReadOnlyList<User>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(u =>
                u.Email.Contains(term) ||
                u.FirstName.Contains(term) ||
                u.LastName.Contains(term));
        }

        return await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(string? search = null, CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(u =>
                u.Email.Contains(term) ||
                u.FirstName.Contains(term) ||
                u.LastName.Contains(term));
        }

        return await query.CountAsync(cancellationToken);
    }

    public async Task<RefreshToken?> GetRefreshTokenAsync(string token, CancellationToken cancellationToken = default) =>
        await Context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);

    public async Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default) =>
        await Context.RefreshTokens.AddAsync(refreshToken, cancellationToken);

    public void UpdateRefreshToken(RefreshToken refreshToken) =>
        Context.RefreshTokens.Update(refreshToken);

    public async Task<PasswordResetToken?> GetPasswordResetTokenAsync(
        string token,
        CancellationToken cancellationToken = default) =>
        await Context.PasswordResetTokens.FirstOrDefaultAsync(prt => prt.Token == token, cancellationToken);

    public async Task AddPasswordResetTokenAsync(
        PasswordResetToken token,
        CancellationToken cancellationToken = default) =>
        await Context.PasswordResetTokens.AddAsync(token, cancellationToken);

    public void UpdatePasswordResetToken(PasswordResetToken token) =>
        Context.PasswordResetTokens.Update(token);

    public async Task RevokeUserRefreshTokensAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var tokens = await Context.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.RevokedAt == null && rt.ExpiresAt > DateTime.UtcNow)
            .ToListAsync(cancellationToken);

        foreach (var token in tokens)
        {
            token.RevokedAt = DateTime.UtcNow;
            token.UpdatedAt = DateTime.UtcNow;
        }
    }
}
