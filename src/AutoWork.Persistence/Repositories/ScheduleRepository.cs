using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Domain.Entities;
using AutoWork.Domain.Enums;
using AutoWork.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace AutoWork.Persistence.Repositories;

public class ScheduleRepository : Repository<PostSchedule>, IScheduleRepository
{
    public ScheduleRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<PostSchedule?> GetByPostIdAsync(Guid postId, CancellationToken cancellationToken = default) =>
        await DbSet
            .Include(ps => ps.Post)
            .FirstOrDefaultAsync(ps => ps.PostId == postId, cancellationToken);

    public async Task<IReadOnlyList<PostSchedule>> GetUpcomingByUserIdAsync(
        Guid userId,
        int count,
        CancellationToken cancellationToken = default) =>
        await DbSet
            .AsNoTracking()
            .Include(ps => ps.Post)
            .ThenInclude(p => p.Project)
            .Where(ps =>
                ps.Post.Project.UserId == userId &&
                ps.Status == (int)PostScheduleStatus.Pending &&
                ps.ScheduledAt >= DateTime.UtcNow)
            .OrderBy(ps => ps.ScheduledAt)
            .Take(count)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<PostSchedule>> GetPendingSchedulesAsync(
        DateTime before,
        CancellationToken cancellationToken = default) =>
        await DbSet
            .Include(ps => ps.Post)
            .ThenInclude(p => p.Contents)
            .Where(ps =>
                ps.Status == (int)PostScheduleStatus.Pending &&
                ps.ScheduledAt <= before)
            .OrderBy(ps => ps.ScheduledAt)
            .ToListAsync(cancellationToken);
}
