using AutoWork.Domain.Entities;

namespace AutoWork.Application.Interfaces.Repositories;

public interface IWordPressRepository : IRepository<WordPressSite>
{
    Task<IReadOnlyList<WordPressSite>> GetSitesByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<WordPressSite?> GetSiteWithPostsAsync(Guid siteId, CancellationToken cancellationToken = default);
    Task<WordPressPost?> GetPostByIdAsync(Guid postId, CancellationToken cancellationToken = default);
    Task<WordPressPost> AddPostAsync(WordPressPost post, CancellationToken cancellationToken = default);
}
