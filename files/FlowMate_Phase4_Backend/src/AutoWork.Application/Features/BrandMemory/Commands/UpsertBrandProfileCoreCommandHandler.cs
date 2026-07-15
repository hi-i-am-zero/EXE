using AutoWork.Application.Common.Exceptions;
using AutoWork.Application.Common.Mappings;
using AutoWork.Application.DTOs.BrandMemory;
using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Application.Interfaces.Services;
using AutoWork.Domain.Entities;
using MediatR;

namespace AutoWork.Application.Features.BrandMemory.Commands;

public class UpsertBrandProfileCoreCommandHandler : IRequestHandler<UpsertBrandProfileCoreCommand, BrandProfileDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IProjectAccessGuard _accessGuard;
    private readonly ICurrentUserService _currentUser;

    public UpsertBrandProfileCoreCommandHandler(
        IUnitOfWork unitOfWork,
        IProjectAccessGuard accessGuard,
        ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _accessGuard = accessGuard;
        _currentUser = currentUser;
    }

    public async Task<BrandProfileDto> Handle(UpsertBrandProfileCoreCommand command, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not Guid userId)
        {
            throw new UnauthorizedException("User is not authenticated.");
        }

        await _accessGuard.EnsureEditorAsync(command.Request.ProjectId, userId, cancellationToken);

        var profile = await _unitOfWork.BrandProfiles.GetByProjectIdWithDetailsAsync(command.Request.ProjectId, cancellationToken);
        var isNew = profile is null;
        profile ??= new BrandProfile { ProjectId = command.Request.ProjectId };

        profile.LogoUrl = command.Request.LogoUrl;
        profile.BusinessName = command.Request.BusinessName.Trim();
        profile.BrandName = command.Request.BrandName.Trim();
        profile.Industry = command.Request.Industry?.Trim();
        profile.FoundingYear = command.Request.FoundingYear;
        profile.ShortDescription = command.Request.ShortDescription?.Trim();
        profile.Address = command.Request.Address?.Trim();
        profile.ContactPhone = command.Request.ContactPhone?.Trim();
        profile.ContactEmail = command.Request.ContactEmail?.Trim();
        profile.Website = command.Request.Website?.Trim();
        profile.VoiceSampleId = command.Request.VoiceSampleId;

        if (isNew)
        {
            await _unitOfWork.BrandProfiles.AddAsync(profile, cancellationToken);
        }
        else
        {
            await _unitOfWork.BrandProfiles.UpdateAsync(profile, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var saved = await _unitOfWork.BrandProfiles.GetByProjectIdWithDetailsAsync(command.Request.ProjectId, cancellationToken)
            ?? profile;
        var channels = await _unitOfWork.BrandProfiles.GetProjectChannelAccountsAsync(command.Request.ProjectId, cancellationToken);

        return BrandProfileMapper.ToDto(saved, channels);
    }
}
