using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Domain.Entities;
using AutoWork.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace AutoWork.Persistence.Repositories;

public class MediaRepository : Repository<MediaFile>, IMediaRepository
{
    public MediaRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<MediaFile>> GetByUserIdAsync(
        Guid userId,
        string? folder = null,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsNoTracking().Where(m => m.UserId == userId);

        if (!string.IsNullOrWhiteSpace(folder))
        {
            query = query.Where(m => m.Project != null && m.Project.Name == folder);
        }

        return await query
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<MediaFile?> GetByStoragePathAsync(
        string storagePath,
        CancellationToken cancellationToken = default) =>
        await DbSet.FirstOrDefaultAsync(m => m.FileUrl == storagePath, cancellationToken);
}
