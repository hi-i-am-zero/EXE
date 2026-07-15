using AutoWork.Application.DTOs.BrandMemory;
using MediatR;

namespace AutoWork.Application.Features.BrandMemory.Commands;

public class SetBrandColorsCommand : IRequest<List<BrandColorDto>>
{
    public SetBrandColorsDto Request { get; set; } = null!;
}
