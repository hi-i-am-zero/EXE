using AutoWork.Domain.Entities;

namespace AutoWork.Application.Interfaces.Repositories;

public interface IFacebookRepository : IRepository<FacebookAccount>
{
    Task<IReadOnlyList<FacebookAccount>> GetAccountsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<FacebookAccount?> GetAccountWithPagesAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FacebookPage>> GetPagesByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<FacebookPage?> GetPageByIdAsync(Guid pageId, CancellationToken cancellationToken = default);
    Task<FacebookPage> AddPageAsync(FacebookPage page, CancellationToken cancellationToken = default);
}
