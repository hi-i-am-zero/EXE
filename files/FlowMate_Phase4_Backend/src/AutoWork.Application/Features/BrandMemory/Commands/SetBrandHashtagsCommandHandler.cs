using AutoWork.Application.Common.Exceptions;
using AutoWork.Application.Common.Helpers;
using AutoWork.Application.DTOs.BrandMemory;
using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Application.Interfaces.Services;
using AutoWork.Domain.Entities;
using MediatR;

namespace AutoWork.Application.Features.BrandMemory.Commands;

public class SetBrandHashtagsCommandHandler : IRequestHandler<SetBrandHashtagsCommand, List<LookupItemDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<BrandHashtag> _joinRepo;
    private readonly IRepository<Hashtag> _hashtags;
    private readonly IProjectAccessGuard _accessGuard;
    private readonly ICurrentUserService _currentUser;

    public SetBrandHashtagsCommandHandler(
        IUnitOfWork unitOfWork,
        IRepository<BrandHashtag> joinRepo,
        IRepository<Hashtag> hashtags,
        IProjectAccessGuard accessGuard,
        ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _joinRepo = joinRepo;
        _hashtags = hashtags;
        _accessGuard = accessGuard;
        _currentUser = currentUser;
    }

    public async Task<List<LookupItemDto>> Handle(SetBrandHashtagsCommand command, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not Guid userId)
        {
            throw new UnauthorizedException("User is not authenticated.");
        }

        await _accessGuard.EnsureEditorAsync(command.Request.ProjectId, userId, cancellationToken);

        var profile = await _unitOfWork.BrandProfiles.GetByProjectIdWithDetailsAsync(command.Request.ProjectId, cancellationToken)
            ?? throw new BadRequestException("Vui lòng lưu Thông tin doanh nghiệp trước khi chọn hashtag.");

        await ManyToManySync.ReplaceSelectionAsync(
            _joinRepo,
            profile.BrandHashtags.ToList(),
            command.Request.SelectedIds,
            hashtagId => new BrandHashtag { BrandProfileId = profile.Id, HashtagId = hashtagId },
            row => row.HashtagId,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var all = await _hashtags.FindAsync(h => command.Request.SelectedIds.Contains(h.Id), cancellationToken);
        return all.Select(h => new LookupItemDto { Id = h.Id, Name = h.Tag }).ToList();
    }
}
