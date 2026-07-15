using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Domain.Entities;
using AutoWork.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace AutoWork.Persistence.Repositories;

public class TimelineRepository : Repository<Timeline>, ITimelineRepository
{
    public TimelineRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Timeline>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default) =>
        await DbSet
            .AsNoTracking()
            .Include(t => t.TemplateType)
            .Include(t => t.Campaign)
            .Where(t => t.ProjectId == projectId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<Timeline?> GetByIdWithPostsAsync(Guid id, CancellationToken cancellationToken = default) =>
        await DbSet
            .Include(t => t.TemplateType)
            .Include(t => t.Campaign)
            .Include(t => t.Posts).ThenInclude(p => p.PostChannelAccounts).ThenInclude(pca => pca.ChannelAccount)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
}
