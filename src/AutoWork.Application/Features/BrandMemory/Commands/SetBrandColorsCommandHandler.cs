using AutoWork.Application.Common.Exceptions;
using AutoWork.Application.DTOs.BrandMemory;
using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Application.Interfaces.Services;
using AutoWork.Domain.Entities;
using MediatR;

namespace AutoWork.Application.Features.BrandMemory.Commands;

/// <summary>Colors/Fonts không phải lookup dùng chung nên không tái dùng ManyToManySync —
/// đơn giản là "xoá hết của profile này rồi tạo lại theo danh sách mới" vì đây là dữ liệu
/// thuộc sở hữu 100% của 1 BrandProfile (không share giữa nhiều profile như Styles/Keywords).</summary>
public class SetBrandColorsCommandHandler : IRequestHandler<SetBrandColorsCommand, List<BrandColorDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<BrandColor> _colors;
    private readonly IProjectAccessGuard _accessGuard;
    private readonly ICurrentUserService _currentUser;

    public SetBrandColorsCommandHandler(
        IUnitOfWork unitOfWork,
        IRepository<BrandColor> colors,
        IProjectAccessGuard accessGuard,
        ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _colors = colors;
        _accessGuard = accessGuard;
        _currentUser = currentUser;
    }

    public async Task<List<BrandColorDto>> Handle(SetBrandColorsCommand command, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not Guid userId)
        {
            throw new UnauthorizedException("User is not authenticated.");
        }

        await _accessGuard.EnsureEditorAsync(command.Request.ProjectId, userId, cancellationToken);

        var profile = await _unitOfWork.BrandProfiles.GetByProjectIdWithDetailsAsync(command.Request.ProjectId, cancellationToken)
            ?? throw new BadRequestException("Vui lòng lưu Thông tin doanh nghiệp trước khi chọn màu thương hiệu.");

        foreach (var existing in profile.Colors.ToList())
        {
            await _colors.DeleteAsync(existing, cancellationToken);
        }

        var created = new List<BrandColor>();
        foreach (var item in command.Request.Colors)
        {
            var color = new BrandColor
            {
                BrandProfileId = profile.Id,
                ColorHex = item.ColorHex,
                ColorType = item.ColorType,
                SortOrder = item.SortOrder
            };
            await _colors.AddAsync(color, cancellationToken);
            created.Add(color);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return created
            .OrderBy(c => c.SortOrder)
            .Select(c => new BrandColorDto { Id = c.Id, ColorHex = c.ColorHex, ColorType = c.ColorType, SortOrder = c.SortOrder })
            .ToList();
    }
}
