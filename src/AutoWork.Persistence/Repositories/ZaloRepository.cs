using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Domain.Entities;
using AutoWork.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace AutoWork.Persistence.Repositories;

public class ZaloRepository : Repository<ZaloAccount>, IZaloRepository
{
    public ZaloRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<ZaloAccount>> GetAccountsByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        await DbSet
            .AsNoTracking()
            .Include(za => za.Posts)
            .Where(za => za.UserId == userId)
            .ToListAsync(cancellationToken);

    public async Task<ZaloAccount?> GetAccountWithPostsAsync(
        Guid accountId,
        CancellationToken cancellationToken = default) =>
        await DbSet
            .Include(za => za.Posts)
            .FirstOrDefaultAsync(za => za.Id == accountId, cancellationToken);

    public async Task<ZaloPost?> GetPostByIdAsync(Guid postId, CancellationToken cancellationToken = default) =>
        await Context.ZaloPosts
            .Include(zp => zp.ZaloAccount)
            .FirstOrDefaultAsync(zp => zp.Id == postId, cancellationToken);

    public async Task<ZaloPost> AddPostAsync(ZaloPost post, CancellationToken cancellationToken = default)
    {
        await Context.ZaloPosts.AddAsync(post, cancellationToken);
        return post;
    }
}
