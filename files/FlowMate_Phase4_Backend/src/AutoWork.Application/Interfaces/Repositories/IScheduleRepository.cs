using AutoWork.Domain.Entities;

namespace AutoWork.Application.Interfaces.Repositories;

public interface IScheduleRepository : IRepository<PostSchedule>
{
    Task<PostSchedule?> GetByPostIdAsync(Guid postId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PostSchedule>> GetUpcomingByUserIdAsync(Guid userId, int count, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PostSchedule>> GetPendingSchedulesAsync(DateTime before, CancellationToken cancellationToken = default);
}
