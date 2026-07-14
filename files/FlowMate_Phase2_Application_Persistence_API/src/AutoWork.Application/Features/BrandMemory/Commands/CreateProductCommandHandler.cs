using AutoWork.Application.Common.Exceptions;
using AutoWork.Application.DTOs.BrandMemory;
using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Application.Interfaces.Services;
using AutoWork.Domain.Entities;
using MediatR;

namespace AutoWork.Application.Features.BrandMemory.Commands;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ProductDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<Product> _products;
    private readonly IRepository<MediaFile> _mediaFiles;
    private readonly IProjectAccessGuard _accessGuard;
    private readonly ICurrentUserService _currentUser;

    public CreateProductCommandHandler(
        IUnitOfWork unitOfWork,
        IRepository<Product> products,
        IRepository<MediaFile> mediaFiles,
        IProjectAccessGuard accessGuard,
        ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _products = products;
        _mediaFiles = mediaFiles;
        _accessGuard = accessGuard;
        _currentUser = currentUser;
    }

    public async Task<ProductDto> Handle(CreateProductCommand command, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not Guid userId)
        {
            throw new UnauthorizedException("User is not authenticated.");
        }

        await _accessGuard.EnsureEditorAsync(command.Request.ProjectId, userId, cancellationToken);

        var profile = await _unitOfWork.BrandProfiles.GetByProjectIdWithDetailsAsync(command.Request.ProjectId, cancellationToken)
            ?? throw new BadRequestException("Vui lòng lưu Thông tin doanh nghiệp trước khi thêm sản phẩm.");

        if (command.Request.ProductCategoryId.HasValue &&
            profile.ProductCategories.All(c => c.Id != command.Request.ProductCategoryId.Value))
        {
            throw new BadRequestException("Danh mục sản phẩm không thuộc workspace này.");
        }

        var product = new Product
        {
            BrandProfileId = profile.Id,
            ProductCategoryId = command.Request.ProductCategoryId,
            ProductName = command.Request.ProductName.Trim(),
            Price = command.Request.Price
        };

        await _products.AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Gắn các ảnh đã upload trước đó (qua MediaController có sẵn) vào sản phẩm vừa tạo
        var imageUrls = new List<string>();
        if (command.Request.MediaFileIds.Count > 0)
        {
            var media = await _mediaFiles.FindAsync(
                m => command.Request.MediaFileIds.Contains(m.Id) && m.ProjectId == command.Request.ProjectId, cancellationToken);

            foreach (var file in media)
            {
                file.ProductId = product.Id;
                await _mediaFiles.UpdateAsync(file, cancellationToken);
                imageUrls.Add(file.FileUrl);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return new ProductDto
        {
            Id = product.Id,
            ProductCategoryId = product.ProductCategoryId,
            ProductName = product.ProductName,
            Price = product.Price,
            AiGeneratedDescription = product.AiGeneratedDescription,
            ImageUrls = imageUrls
        };
    }
}
