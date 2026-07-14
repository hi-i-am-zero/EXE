using AutoWork.Application.DTOs.Timelines;
using AutoWork.Application.Features.Timelines.Commands;
using AutoWork.Application.Features.Timelines.Queries;
using AutoWork.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoWork.API.Controllers;

/// <summary>FlowMate v2: hoàn toàn mới, không có ở bản AutoWork cũ.</summary>
[Authorize]
[Route("api/timelines")]
public class TimelinesController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public TimelinesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("projects/{projectId:guid}")]
    public async Task<ActionResult<ApiResponse<List<TimelineDto>>>> GetTimelines(Guid projectId)
    {
        var result = await _mediator.Send(new GetTimelinesQuery { ProjectId = projectId });
        return OkResponse(result);
    }

    [HttpPost("projects/{projectId:guid}")]
    public async Task<ActionResult<ApiResponse<TimelineDto>>> CreateTimeline(
        Guid projectId, [FromBody] CreateTimelineDto request)
    {
        request.ProjectId = projectId;
        var result = await _mediator.Send(new CreateTimelineCommand { Request = request });
        return OkResponse(result, "Đã tạo timeline.");
    }
}
