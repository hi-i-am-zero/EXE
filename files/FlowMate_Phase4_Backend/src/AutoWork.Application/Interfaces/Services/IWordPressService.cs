using AutoWork.Application.DTOs.WordPress;

namespace AutoWork.Application.Interfaces.Services;

public interface IWordPressService
{
    Task<bool> TestConnectionAsync(WordPressSiteDto site, CancellationToken cancellationToken = default);
    Task<string> PublishPostAsync(Guid userId, CreateWordPressPostDto request, CancellationToken cancellationToken = default);
    Task<WordPressPostDto> GetPostAsync(Guid siteId, string externalPostId, CancellationToken cancellationToken = default);
}
