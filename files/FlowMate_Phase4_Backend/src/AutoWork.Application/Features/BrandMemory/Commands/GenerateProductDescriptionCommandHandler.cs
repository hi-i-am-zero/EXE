using AutoWork.Application.Common.Exceptions;
using AutoWork.Application.DTOs.AI;
using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Application.Interfaces.Services;
using AutoWork.Domain.Entities;
using MediatR;

namespace AutoWork.Application.Features.BrandMemory.Commands;

/// <summary>Bọc IAiContentService.GenerateProductDescriptionAsync bằng CQRS cho nhất quán với
/// các Command khác — đồng thời kiểm tra quyền truy cập workspace trước khi gọi AI (tốn credit),
/// vì IAiContentService chỉ biết ProductId, không biết về Project/quyền truy cập.</summary>
public class GenerateProductDescriptionCommandHandler
    : IRequestHandler<GenerateProductDescriptionCommand, GenerateProductDescriptionResponse>
{
    private readonly IProductVisionService _visionService;
    private readonly IRepository<Product> _products;
    private readonly IRepository<BrandProfile> _brandProfiles;
    private readonly IProjectAccessGuard _accessGuard;
    private readonly ICurrentUserService _currentUser;

    public GenerateProductDescriptionCommandHandler(
        IProductVisionService visionService,
        IRepository<Product> products,
        IRepository<BrandProfile> brandProfiles,
        IProjectAccessGuard accessGuard,
        ICurrentUserService currentUser)
    {
        _visionService = visionService;
        _products = products;
        _brandProfiles = brandProfiles;
        _accessGuard = accessGuard;
        _currentUser = currentUser;
    }

    public async Task<GenerateProductDescriptionResponse> Handle(
        GenerateProductDescriptionCommand command, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not Guid userId)
        {
            throw new UnauthorizedException("User is not authenticated.");
        }

        var product = await _products.GetByIdAsync(command.Request.ProductId, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), command.Request.ProductId);

        var brandProfile = await _brandProfiles.GetByIdAsync(product.BrandProfileId, cancellationToken)
            ?? throw new NotFoundException(nameof(BrandProfile), product.BrandProfileId);

        await _accessGuard.EnsureEditorAsync(brandProfile.ProjectId, userId, cancellationToken);

        return await _visionService.GenerateProductDescriptionAsync(userId, command.Request, cancellationToken);
    }
}
