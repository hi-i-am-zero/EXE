using AutoWork.Application.DTOs.BrandMemory;
using MediatR;

namespace AutoWork.Application.Features.BrandMemory.Commands;

public class CreateProductCategoryCommand : IRequest<ProductCategoryDto>
{
    public CreateProductCategoryDto Request { get; set; } = null!;
}
