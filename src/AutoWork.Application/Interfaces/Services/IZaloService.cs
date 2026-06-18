using AutoWork.Application.DTOs.Zalo;

namespace AutoWork.Application.Interfaces.Services;

public interface IZaloService
{
    Task<string> GetAuthorizationUrlAsync(Guid userId, string redirectUri, CancellationToken cancellationToken = default);
    Task<ZaloAccountDto> ConnectAccountAsync(Guid userId, string authorizationCode, CancellationToken cancellationToken = default);
    Task<string> PublishPostAsync(Guid userId, ZaloPostDto request, CancellationToken cancellationToken = default);
}
