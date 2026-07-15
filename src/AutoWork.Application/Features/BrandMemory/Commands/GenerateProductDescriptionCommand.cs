using AutoWork.Application.DTOs.AI;
using MediatR;

namespace AutoWork.Application.Features.BrandMemory.Commands;

public class GenerateProductDescriptionCommand : IRequest<GenerateProductDescriptionResponse>
{
    public GenerateProductDescriptionRequest Request { get; set; } = null!;
}
