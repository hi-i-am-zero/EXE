using AutoWork.Application.DTOs.BrandMemory;
using MediatR;

namespace AutoWork.Application.Features.BrandMemory.Commands;

public class SetBrandFontsCommand : IRequest<List<BrandFontDto>>
{
    public SetBrandFontsDto Request { get; set; } = null!;
}
