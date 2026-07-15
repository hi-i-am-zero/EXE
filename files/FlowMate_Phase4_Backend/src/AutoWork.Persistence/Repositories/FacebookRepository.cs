using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Domain.Entities;
using AutoWork.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace AutoWork.Persistence.Repositories;

public class FacebookRepository : Repository<FacebookAccount>, IFacebookRepository
{
    public FacebookRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<FacebookAccount>> GetAccountsByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        await DbSet
            .AsNoTracking()
            .Include(fa => fa.Pages)
            .Where(fa => fa.UserId == userId)
            .ToListAsync(cancellationToken);

    public async Task<FacebookAccount?> GetAccountWithPagesAsync(
        Guid accountId,
        CancellationToken cancellationToken = default) =>
        await DbSet
            .Include(fa => fa.Pages)
            .FirstOrDefaultAsync(fa => fa.Id == accountId, cancellationToken);

    public async Task<IReadOnlyList<FacebookPage>> GetPagesByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        await Context.FacebookPages
            .AsNoTracking()
            .Include(fp => fp.FacebookAccount)
            .Where(fp => fp.FacebookAccount.UserId == userId)
            .ToListAsync(cancellationToken);

    public async Task<FacebookPage?> GetPageByIdAsync(Guid pageId, CancellationToken cancellationToken = default) =>
        await Context.FacebookPages
            .Include(fp => fp.FacebookAccount)
            .FirstOrDefaultAsync(fp => fp.Id == pageId, cancellationToken);

    public async Task<FacebookPage> AddPageAsync(FacebookPage page, CancellationToken cancellationToken = default)
    {
        await Context.FacebookPages.AddAsync(page, cancellationToken);
        return page;
    }
}
