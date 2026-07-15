using AutoWork.Application.DTOs.AI;
using MediatR;

namespace AutoWork.Application.Features.Campaigns.Commands;

public class GenerateTimelineOptionsCommand : IRequest<GenerateTimelineOptionsResponse>
{
    public GenerateTimelineOptionsRequest Request { get; set; } = null!;
}
