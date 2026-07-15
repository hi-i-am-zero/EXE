using AutoWork.Application.Common.Exceptions;
using AutoWork.Application.DTOs.Channels;
using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Application.Interfaces.Services;
using MediatR;

namespace AutoWork.Application.Features.Channels.Queries;

public class GetChannelAccountsQueryHandler : IRequestHandler<GetChannelAccountsQuery, List<ChannelAccountDto>>
{
    private readonly IBrandProfileRepository _brandProfiles; // tái dùng GetProjectChannelAccountsAsync đã có (Include Channel)
    private readonly IProjectAccessGuard _accessGuard;
    private readonly ICurrentUserService _currentUser;

    public GetChannelAccountsQueryHandler(
        IBrandProfileRepository brandProfiles,
        IProjectAccessGuard accessGuard,
        ICurrentUserService currentUser)
    {
        _brandProfiles = brandProfiles;
        _accessGuard = accessGuard;
        _currentUser = currentUser;
    }

    public async Task<List<ChannelAccountDto>> Handle(GetChannelAccountsQuery request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not Guid userId)
        {
            throw new UnauthorizedException("User is not authenticated.");
        }

        await _accessGuard.EnsureViewerAsync(request.ProjectId, userId, cancellationToken);

        var accounts = await _brandProfiles.GetProjectChannelAccountsAsync(request.ProjectId, cancellationToken);
        return accounts.Select(ca => new ChannelAccountDto
        {
            Id = ca.Id,
            ChannelId = ca.ChannelId,
            ChannelCode = ca.Channel.Code,
            ChannelName = ca.Channel.Name,
            Name = ca.Name,
            ProfileUrl = ca.ProfileUrl,
            AvatarUrl = ca.AvatarUrl,
            IsActive = ca.IsActive,
            HasApiConnection = !string.IsNullOrEmpty(ca.AccessToken)
        }).ToList();
    }
}
