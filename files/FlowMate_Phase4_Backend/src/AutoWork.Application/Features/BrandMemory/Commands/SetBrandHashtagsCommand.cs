using AutoWork.Application.DTOs.BrandMemory;
using MediatR;

namespace AutoWork.Application.Features.BrandMemory.Commands;

public class SetBrandHashtagsCommand : IRequest<List<LookupItemDto>>
{
    public SetBrandTagsDto Request { get; set; } = null!;
}
