using AutoWork.Application.DTOs.Posts;
using AutoWork.Application.DTOs.Schedules;
using AutoWork.Application.Features.Schedules.Commands;
using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Application.Interfaces.Services;
using AutoWork.Domain.Enums;
using AutoWork.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoWork.API.Controllers;

[Authorize]
public class SchedulesController : ApiControllerBase
{
    private readonly IMediator _mediator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public SchedulesController(IMediator mediator, IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<ScheduleDto>>>> GetSchedules([FromQuery] int count = 50)
    {
        var schedules = await _unitOfWork.Schedules.GetUpcomingByUserIdAsync(_currentUser.UserId!.Value, count);
        return OkResponse(schedules.Select(s => new ScheduleDto
        {
            Id = s.Id,
            PostId = s.PostId,
            ScheduledAt = s.ScheduledAt,
            RecurrenceType = 0,
            IsProcessed = s.Status != (int)PostScheduleStatus.Pending
        }).ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ScheduleDto>>> GetSchedule(Guid id)
    {
        var schedule = await _unitOfWork.Schedules.GetByIdAsync(id)
            ?? throw new Application.Common.Exceptions.NotFoundException("Schedule", id);

        return OkResponse(new ScheduleDto
        {
            Id = schedule.Id,
            PostId = schedule.PostId,
            ScheduledAt = schedule.ScheduledAt,
            IsProcessed = schedule.Status != (int)PostScheduleStatus.Pending
        });
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<PostScheduleDto>>> CreateSchedule([FromBody] CreateScheduleRequest request)
    {
        var result = await _mediator.Send(new CreateScheduleCommand
        {
            PostId = request.PostId,
            ScheduledAt = request.ScheduledAt
        });
        return OkResponse(result, "Schedule created.");
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ScheduleDto>>> UpdateSchedule(Guid id, [FromBody] UpdateScheduleRequest request)
    {
        var schedule = await _unitOfWork.Schedules.GetByIdAsync(id)
            ?? throw new Application.Common.Exceptions.NotFoundException("Schedule", id);

        schedule.ScheduledAt = request.ScheduledAt;
        schedule.Status = (int)PostScheduleStatus.Pending;
        await _unitOfWork.Schedules.UpdateAsync(schedule);
        await _unitOfWork.SaveChangesAsync();

        return OkResponse(new ScheduleDto { Id = schedule.Id, PostId = schedule.PostId, ScheduledAt = schedule.ScheduledAt }, "Schedule updated.");
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse>> DeleteSchedule(Guid id)
    {
        var schedule = await _unitOfWork.Schedules.GetByIdAsync(id)
            ?? throw new Application.Common.Exceptions.NotFoundException("Schedule", id);

        schedule.Status = (int)PostScheduleStatus.Cancelled;
        await _unitOfWork.Schedules.UpdateAsync(schedule);
        await _unitOfWork.SaveChangesAsync();
        return OkResponse("Schedule cancelled.");
    }
}
