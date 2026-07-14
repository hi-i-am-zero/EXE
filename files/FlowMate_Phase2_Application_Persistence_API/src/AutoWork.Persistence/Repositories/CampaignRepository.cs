using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Domain.Entities;
using AutoWork.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace AutoWork.Persistence.Repositories;

public class CampaignRepository : Repository<Campaign>, ICampaignRepository
{
    public CampaignRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Campaign>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default) =>
        await DbSet
            .AsNoTracking()
            .Include(c => c.Goal)
            .Include(c => c.PromotionType)
            .Where(c => c.ProjectId == projectId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<Campaign?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default) =>
        await DbSet
            .Include(c => c.Goal)
            .Include(c => c.PromotionType)
            .Include(c => c.CampaignChannelAccounts)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
}
