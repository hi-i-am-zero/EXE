using AutoWork.Application.DTOs.AI;
using MediatR;

namespace AutoWork.Application.Features.AI.Commands;

public class GenerateContentCommand : IRequest<GenerateContentResponse>
{
    public GenerateContentRequest Request { get; set; } = null!;
}
