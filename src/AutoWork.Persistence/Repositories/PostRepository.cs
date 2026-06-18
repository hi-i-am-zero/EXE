using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Domain.Entities;
using AutoWork.Domain.Enums;
using AutoWork.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace AutoWork.Persistence.Repositories;

public class PostRepository : Repository<Post>, IPostRepository
{
    public PostRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Post?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default) =>
        await DbSet
            .Include(p => p.Contents)
            .Include(p => p.Schedule)
            .Include(p => p.Logs)
            .Include(p => p.ChannelAccount)
            .ThenInclude(ca => ca.Channel)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Post>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
        await DbSet
            .AsNoTracking()
            .Include(p => p.Project)
            .Where(p => p.Project.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Post>> GetPagedAsync(
        Guid userId,
        int pageNumber,
        int pageSize,
        int? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .AsNoTracking()
            .Include(p => p.Project)
            .Where(p => p.Project.UserId == userId);

        if (status.HasValue)
        {
            query = query.Where(p => p.Status == status.Value);
        }

        return await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountByUserIdAsync(
        Guid userId,
        int? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.Where(p => p.Project.UserId == userId);

        if (status.HasValue)
        {
            query = query.Where(p => p.Status == status.Value);
        }

        return await query.CountAsync(cancellationToken);
    }

    public async Task<int> CountPublishedTodayAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        return await DbSet.CountAsync(
            p => p.Project.UserId == userId &&
                 p.Status == (int)PostStatus.Published &&
                 p.PublishedAt >= today &&
                 p.PublishedAt < tomorrow,
            cancellationToken);
    }
}
