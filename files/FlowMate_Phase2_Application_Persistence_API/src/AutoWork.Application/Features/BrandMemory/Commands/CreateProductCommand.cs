using AutoWork.Application.DTOs.BrandMemory;
using MediatR;

namespace AutoWork.Application.Features.BrandMemory.Commands;

public class CreateProductCommand : IRequest<ProductDto>
{
    public CreateProductDto Request { get; set; } = null!;
}
