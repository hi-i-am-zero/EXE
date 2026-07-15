using AutoWork.Application.Common.Exceptions;
using AutoWork.Application.Common.Helpers;
using AutoWork.Application.DTOs.BrandMemory;
using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Application.Interfaces.Services;
using AutoWork.Domain.Entities;
using MediatR;

namespace AutoWork.Application.Features.BrandMemory.Commands;

public class SetBrandStylesCommandHandler : IRequestHandler<SetBrandStylesCommand, List<LookupItemDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<BrandProfileStyle> _joinRepo;
    private readonly IRepository<BrandStyle> _styles;
    private readonly IProjectAccessGuard _accessGuard;
    private readonly ICurrentUserService _currentUser;

    public SetBrandStylesCommandHandler(
        IUnitOfWork unitOfWork,
        IRepository<BrandProfileStyle> joinRepo,
        IRepository<BrandStyle> styles,
        IProjectAccessGuard accessGuard,
        ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _joinRepo = joinRepo;
        _styles = styles;
        _accessGuard = accessGuard;
        _currentUser = currentUser;
    }

    public async Task<List<LookupItemDto>> Handle(SetBrandStylesCommand command, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not Guid userId)
        {
            throw new UnauthorizedException("User is not authenticated.");
        }

        await _accessGuard.EnsureEditorAsync(command.Request.ProjectId, userId, cancellationToken);

        var profile = await _unitOfWork.BrandProfiles.GetByProjectIdWithDetailsAsync(command.Request.ProjectId, cancellationToken)
            ?? throw new BadRequestException("Vui lòng lưu Thông tin doanh nghiệp trước khi chọn phong cách.");

        await ManyToManySync.ReplaceSelectionAsync(
            _joinRepo,
            profile.BrandProfileStyles.ToList(),
            command.Request.SelectedIds,
            styleId => new BrandProfileStyle { BrandProfileId = profile.Id, StyleId = styleId },
            row => row.StyleId,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var allStyles = await _styles.FindAsync(s => command.Request.SelectedIds.Contains(s.Id), cancellationToken);
        return allStyles.Select(s => new LookupItemDto { Id = s.Id, Name = s.StyleName }).ToList();
    }
}
