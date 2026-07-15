using AutoWork.Application.DTOs.AI;
using AutoWork.Application.DTOs.Timelines;
using MediatR;

namespace AutoWork.Application.Features.Campaigns.Commands;

public class ConfirmTimelineOptionCommand : IRequest<TimelineDto>
{
    public ConfirmTimelineOptionRequest Request { get; set; } = null!;
}
