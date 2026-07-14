using AutoWork.Application.DTOs.Posts;
using MediatR;

namespace AutoWork.Application.Features.Schedules.Commands;

public class CreateScheduleCommand : IRequest<PostScheduleDto>
{
    public Guid PostId { get; set; }
    public DateTime ScheduledAt { get; set; }
}
