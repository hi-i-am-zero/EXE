using AutoWork.Application.DTOs.BrandMemory;
using MediatR;

namespace AutoWork.Application.Features.BrandMemory.Queries;

public class GetBrandProfileQuery : IRequest<BrandProfileDto>
{
    public Guid ProjectId { get; set; }
}
