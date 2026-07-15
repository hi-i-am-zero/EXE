using AutoWork.Domain.Entities;

namespace AutoWork.Application.Interfaces.Repositories;

public interface IPostRepository : IRepository<Post>
{
    Task<Post?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Post>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Post>> GetPagedAsync(Guid userId, int pageNumber, int pageSize, int? status = null, CancellationToken cancellationToken = default);
    Task<int> CountByUserIdAsync(Guid userId, int? status = null, CancellationToken cancellationToken = default);
    Task<int> CountPublishedTodayAsync(Guid userId, CancellationToken cancellationToken = default);
}
