using AutoWork.Application.Common.Exceptions;
using AutoWork.Application.DTOs.BrandMemory;
using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Application.Interfaces.Services;
using AutoWork.Domain.Entities;
using MediatR;

namespace AutoWork.Application.Features.BrandMemory.Commands;

public class CreateProductCategoryCommandHandler : IRequestHandler<CreateProductCategoryCommand, ProductCategoryDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<ProductCategory> _categories;
    private readonly IProjectAccessGuard _accessGuard;
    private readonly ICurrentUserService _currentUser;

    public CreateProductCategoryCommandHandler(
        IUnitOfWork unitOfWork,
        IRepository<ProductCategory> categories,
        IProjectAccessGuard accessGuard,
        ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _categories = categories;
        _accessGuard = accessGuard;
        _currentUser = currentUser;
    }

    public async Task<ProductCategoryDto> Handle(CreateProductCategoryCommand command, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not Guid userId)
        {
            throw new UnauthorizedException("User is not authenticated.");
        }

        await _accessGuard.EnsureEditorAsync(command.Request.ProjectId, userId, cancellationToken);

        var profile = await _unitOfWork.BrandProfiles.GetByProjectIdWithDetailsAsync(command.Request.ProjectId, cancellationToken)
            ?? throw new BadRequestException("Vui lòng lưu Thông tin doanh nghiệp trước khi thêm danh mục sản phẩm.");

        var name = command.Request.CategoryName.Trim();
        if (profile.ProductCategories.Any(c => string.Equals(c.CategoryName, name, StringComparison.OrdinalIgnoreCase)))
        {
            throw new BadRequestException("Danh mục sản phẩm này đã tồn tại.");
        }

        var category = new ProductCategory { BrandProfileId = profile.Id, CategoryName = name };
        await _categories.AddAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ProductCategoryDto { Id = category.Id, CategoryName = category.CategoryName, ProductCount = 0 };
    }
}
