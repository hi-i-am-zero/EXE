using AutoWork.Application.Common.Exceptions;
using AutoWork.Application.Common.Mappings;
using AutoWork.Application.DTOs.BrandMemory;
using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Application.Interfaces.Services;
using MediatR;

namespace AutoWork.Application.Features.BrandMemory.Queries;

public class GetBrandProfileQueryHandler : IRequestHandler<GetBrandProfileQuery, BrandProfileDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IProjectAccessGuard _accessGuard;
    private readonly ICurrentUserService _currentUser;

    public GetBrandProfileQueryHandler(IUnitOfWork unitOfWork, IProjectAccessGuard accessGuard, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _accessGuard = accessGuard;
        _currentUser = currentUser;
    }

    public async Task<BrandProfileDto> Handle(GetBrandProfileQuery request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not Guid userId)
        {
            throw new UnauthorizedException("User is not authenticated.");
        }

        await _accessGuard.EnsureViewerAsync(request.ProjectId, userId, cancellationToken);

        var profile = await _unitOfWork.BrandProfiles.GetByProjectIdWithDetailsAsync(request.ProjectId, cancellationToken);
        if (profile is null)
        {
            return BrandProfileMapper.Empty(request.ProjectId);
        }

        var channels = await _unitOfWork.BrandProfiles.GetProjectChannelAccountsAsync(request.ProjectId, cancellationToken);
        return BrandProfileMapper.ToDto(profile, channels);
    }
}
