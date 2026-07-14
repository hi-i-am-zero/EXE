using AutoWork.Domain.Entities;

namespace AutoWork.Application.Interfaces.Repositories;

public interface IZaloRepository : IRepository<ZaloAccount>
{
    Task<IReadOnlyList<ZaloAccount>> GetAccountsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ZaloAccount?> GetAccountWithPostsAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task<ZaloPost?> GetPostByIdAsync(Guid postId, CancellationToken cancellationToken = default);
    Task<ZaloPost> AddPostAsync(ZaloPost post, CancellationToken cancellationToken = default);
}
