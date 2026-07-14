using AutoWork.Application.Common.Exceptions;
using AutoWork.Application.Common.Helpers;
using AutoWork.Application.DTOs.BrandMemory;
using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Application.Interfaces.Services;
using AutoWork.Domain.Entities;
using MediatR;

namespace AutoWork.Application.Features.BrandMemory.Commands;

public class SetBrandCtasCommandHandler : IRequestHandler<SetBrandCtasCommand, List<LookupItemDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<BrandCta> _joinRepo;
    private readonly IRepository<CtaTemplate> _ctas;
    private readonly IProjectAccessGuard _accessGuard;
    private readonly ICurrentUserService _currentUser;

    public SetBrandCtasCommandHandler(
        IUnitOfWork unitOfWork,
        IRepository<BrandCta> joinRepo,
        IRepository<CtaTemplate> ctas,
        IProjectAccessGuard accessGuard,
        ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _joinRepo = joinRepo;
        _ctas = ctas;
        _accessGuard = accessGuard;
        _currentUser = currentUser;
    }

    public async Task<List<LookupItemDto>> Handle(SetBrandCtasCommand command, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not Guid userId)
        {
            throw new UnauthorizedException("User is not authenticated.");
        }

        await _accessGuard.EnsureEditorAsync(command.Request.ProjectId, userId, cancellationToken);

        var profile = await _unitOfWork.BrandProfiles.GetByProjectIdWithDetailsAsync(command.Request.ProjectId, cancellationToken)
            ?? throw new BadRequestException("Vui lòng lưu Thông tin doanh nghiệp trước khi chọn CTA.");

        await ManyToManySync.ReplaceSelectionAsync(
            _joinRepo,
            profile.BrandCtas.ToList(),
            command.Request.SelectedIds,
            ctaId => new BrandCta { BrandProfileId = profile.Id, CtaId = ctaId },
            row => row.CtaId,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var all = await _ctas.FindAsync(c => command.Request.SelectedIds.Contains(c.Id), cancellationToken);
        return all.Select(c => new LookupItemDto { Id = c.Id, Name = c.CtaText }).ToList();
    }
}
