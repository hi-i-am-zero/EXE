using AutoWork.Domain.Entities;

namespace AutoWork.Application.Interfaces.Repositories;

public interface IPendingRegistrationRepository : IRepository<PendingRegistration>
{
    Task<PendingRegistration?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);

    Task<PendingRegistration?> GetActiveByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<PendingRegistration?> GetActiveByPhoneAsync(string phone, CancellationToken cancellationToken = default);

    Task RemoveActiveByEmailAsync(string email, CancellationToken cancellationToken = default);
}
