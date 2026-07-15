using AutoWork.Application.DTOs.Timelines;
using MediatR;

namespace AutoWork.Application.Features.Timelines.Queries;

public class GetTimelinesQuery : IRequest<List<TimelineDto>>
{
    public Guid ProjectId { get; set; }
}
