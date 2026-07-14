using AutoWork.Application.DTOs.Timelines;
using MediatR;

namespace AutoWork.Application.Features.Timelines.Commands;

public class CreateTimelineCommand : IRequest<TimelineDto>
{
    public CreateTimelineDto Request { get; set; } = null!;
}
