using AutoWork.Application.Common.Exceptions;
using AutoWork.Application.DTOs.BrandMemory;
using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Application.Interfaces.Services;
using AutoWork.Domain.Entities;
using MediatR;

namespace AutoWork.Application.Features.BrandMemory.Commands;

public class SetBrandFontsCommandHandler : IRequestHandler<SetBrandFontsCommand, List<BrandFontDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<BrandFont> _fonts;
    private readonly IProjectAccessGuard _accessGuard;
    private readonly ICurrentUserService _currentUser;

    public SetBrandFontsCommandHandler(
        IUnitOfWork unitOfWork,
        IRepository<BrandFont> fonts,
        IProjectAccessGuard accessGuard,
        ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _fonts = fonts;
        _accessGuard = accessGuard;
        _currentUser = currentUser;
    }

    public async Task<List<BrandFontDto>> Handle(SetBrandFontsCommand command, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not Guid userId)
        {
            throw new UnauthorizedException("User is not authenticated.");
        }

        await _accessGuard.EnsureEditorAsync(command.Request.ProjectId, userId, cancellationToken);

        var profile = await _unitOfWork.BrandProfiles.GetByProjectIdWithDetailsAsync(command.Request.ProjectId, cancellationToken)
            ?? throw new BadRequestException("Vui lòng lưu Thông tin doanh nghiệp trước khi chọn font thương hiệu.");

        foreach (var existing in profile.Fonts.ToList())
        {
            await _fonts.DeleteAsync(existing, cancellationToken);
        }

        var created = new List<BrandFont>();
        foreach (var item in command.Request.Fonts)
        {
            var font = new BrandFont { BrandProfileId = profile.Id, FontName = item.FontName.Trim(), UsageType = item.UsageType };
            await _fonts.AddAsync(font, cancellationToken);
            created.Add(font);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return created.Select(f => new BrandFontDto { Id = f.Id, FontName = f.FontName, UsageType = f.UsageType }).ToList();
    }
}
