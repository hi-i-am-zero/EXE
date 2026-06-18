using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Domain.Entities;
using AutoWork.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace AutoWork.Persistence.Repositories;

public class ProjectRepository : Repository<Project>, IProjectRepository
{
    public ProjectRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Project>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        await DbSet
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<Project?> GetByIdWithPostsAsync(Guid id, CancellationToken cancellationToken = default) =>
        await DbSet
            .Include(p => p.Posts)
            .Include(p => p.ChannelAccounts)
            .ThenInclude(ca => ca.Channel)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<int> CountByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
        await DbSet.CountAsync(p => p.UserId == userId, cancellationToken);
}
