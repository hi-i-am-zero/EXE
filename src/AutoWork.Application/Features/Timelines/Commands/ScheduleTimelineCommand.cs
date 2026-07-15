using AutoWork.Application.DTOs.Timelines;
using MediatR;

namespace AutoWork.Application.Features.Timelines.Commands;

public class ScheduleTimelineCommand : IRequest<ScheduleTimelineResponse>
{
    public ScheduleTimelineRequest Request { get; set; } = null!;
}
