using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Domain.Entities;
using AutoWork.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace AutoWork.Persistence.Repositories;

public class WordPressRepository : Repository<WordPressSite>, IWordPressRepository
{
    public WordPressRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<WordPressSite>> GetSitesByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        await DbSet
            .AsNoTracking()
            .Where(ws => ws.UserId == userId)
            .ToListAsync(cancellationToken);

    public async Task<WordPressSite?> GetSiteWithPostsAsync(
        Guid siteId,
        CancellationToken cancellationToken = default) =>
        await DbSet
            .Include(ws => ws.Posts)
            .FirstOrDefaultAsync(ws => ws.Id == siteId, cancellationToken);

    public async Task<WordPressPost?> GetPostByIdAsync(Guid postId, CancellationToken cancellationToken = default) =>
        await Context.WordPressPosts
            .Include(wp => wp.WordPressSite)
            .FirstOrDefaultAsync(wp => wp.Id == postId, cancellationToken);

    public async Task<WordPressPost> AddPostAsync(WordPressPost post, CancellationToken cancellationToken = default)
    {
        await Context.WordPressPosts.AddAsync(post, cancellationToken);
        return post;
    }
}
