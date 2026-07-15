using AutoWork.Domain.Entities;

namespace AutoWork.Application.Interfaces.Repositories;

public interface ITimelineRepository : IRepository<Timeline>
{
    Task<IReadOnlyList<Timeline>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);

    Task<Timeline?> GetByIdWithPostsAsync(Guid id, CancellationToken cancellationToken = default);
}
