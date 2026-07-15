using AutoWork.Application.DTOs.BrandMemory;
using MediatR;

namespace AutoWork.Application.Features.BrandMemory.Commands;

public class SetBrandCtasCommand : IRequest<List<LookupItemDto>>
{
    public SetBrandTagsDto Request { get; set; } = null!;
}
