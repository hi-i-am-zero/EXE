using AutoWork.Domain.Entities;

namespace AutoWork.Application.Interfaces.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByIdWithRolesAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByReferralCodeAsync(string referralCode, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<User>> GetPagedAsync(int pageNumber, int pageSize, string? search = null, CancellationToken cancellationToken = default);
    Task<int> CountAsync(string? search = null, CancellationToken cancellationToken = default);
    Task<RefreshToken?> GetRefreshTokenAsync(string token, CancellationToken cancellationToken = default);
    Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
    void UpdateRefreshToken(RefreshToken refreshToken);
    Task<PasswordResetToken?> GetPasswordResetTokenAsync(string token, CancellationToken cancellationToken = default);
    Task AddPasswordResetTokenAsync(PasswordResetToken token, CancellationToken cancellationToken = default);
    void UpdatePasswordResetToken(PasswordResetToken token);
    Task RevokeUserRefreshTokensAsync(Guid userId, CancellationToken cancellationToken = default);
}
