using AutoWork.Application.DTOs.Timelines;
using AutoWork.Application.Features.Timelines.Commands;
using AutoWork.Application.Features.Timelines.Queries;
using AutoWork.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoWork.API.Controllers;

/// <summary>FlowMate v2: hoàn toàn mới, không có ở bản AutoWork cũ.
/// Lưu ý: việc SINH nội dung timeline bằng AI đã chuyển sang CampaignsController
/// (GenerateTimelineOptions + ConfirmTimelineOption) đúng luồng "AI sinh 3 phương án" —
/// Controller này chỉ còn CRUD Timeline cơ bản + hành động lên lịch đăng hàng loạt.</summary>
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

    /// <summary>Bước cuối luồng Chiến dịch tự động: "Đăng bài" — xác nhận CẢ timeline, tự tạo lịch
    /// đăng cho từng bài theo đúng ngày AI đã đề xuất. Bài đầu tiên có thể chọn đăng ngay lập tức.</summary>
    [HttpPost("{timelineId:guid}/schedule")]
    public async Task<ActionResult<ApiResponse<ScheduleTimelineResponse>>> ScheduleTimeline(
        Guid timelineId, [FromBody] ScheduleTimelineBodyDto request)
    {
        var result = await _mediator.Send(new ScheduleTimelineCommand
        {
            Request = new ScheduleTimelineRequest { TimelineId = timelineId, PublishFirstImmediately = request.PublishFirstImmediately }
        });
        return OkResponse(result, "Đã lên lịch đăng bài cho toàn bộ timeline.");
    }
}

public class ScheduleTimelineBodyDto
{
    public bool PublishFirstImmediately { get; set; }
}
