using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Domain.Entities;
using AutoWork.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace AutoWork.Persistence.Repositories;

public class BrandProfileRepository : Repository<BrandProfile>, IBrandProfileRepository
{
    public BrandProfileRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<BrandProfile?> GetByProjectIdWithDetailsAsync(Guid projectId, CancellationToken cancellationToken = default) =>
        await DbSet
            .Include(b => b.VoiceSample)
            .Include(b => b.BrandProfileStyles).ThenInclude(x => x.Style)
            .Include(b => b.BrandProfileKeywords).ThenInclude(x => x.ToneKeyword)
            .Include(b => b.BrandCtas).ThenInclude(x => x.Cta)
            .Include(b => b.BrandHashtags).ThenInclude(x => x.Hashtag)
            .Include(b => b.Colors)
            .Include(b => b.Fonts)
            .Include(b => b.ProductCategories).ThenInclude(c => c.Products)
            .FirstOrDefaultAsync(b => b.ProjectId == projectId, cancellationToken);

    public async Task<IReadOnlyList<ChannelAccount>> GetProjectChannelAccountsAsync(Guid projectId, CancellationToken cancellationToken = default) =>
        await Context.Set<ChannelAccount>()
            .AsNoTracking()
            .Include(ca => ca.Channel)
            .Where(ca => ca.ProjectId == projectId)
            .ToListAsync(cancellationToken);
}
