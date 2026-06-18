using AutoWork.Application.DTOs.Facebook;

namespace AutoWork.Application.Interfaces.Services;

public interface IFacebookService
{
    Task<string> GetAuthorizationUrlAsync(Guid userId, string redirectUri, CancellationToken cancellationToken = default);
    Task<FacebookAccountDto> ConnectAccountAsync(Guid userId, string authorizationCode, string redirectUri, CancellationToken cancellationToken = default);
    Task<string> PublishPostAsync(Guid userId, PublishFacebookPostDto request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FacebookPageDto>> SyncPagesAsync(Guid userId, Guid accountId, CancellationToken cancellationToken = default);
}
