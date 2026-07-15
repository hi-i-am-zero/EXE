using AutoWork.Application.DTOs.AI;
using AutoWork.Application.DTOs.Timelines;
using AutoWork.Application.Features.Timelines.Commands;
using AutoWork.Application.Features.Timelines.Queries;
using AutoWork.Shared.Enums;
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

    /// <summary>AI sinh bộ bài viết nháp (title/content/hashtag) rải đều theo khung thời gian Timeline —
    /// đúng tính năng "gợi ý timeline & content mẫu" cho lịch đăng bài tự động.</summary>
    [HttpPost("{timelineId:guid}/generate-posts")]
    public async Task<ActionResult<ApiResponse<GenerateTimelinePostsResponse>>> GenerateTimelinePosts(
        Guid timelineId, [FromBody] GenerateTimelinePostsBodyDto request)
    {
        var result = await _mediator.Send(new GenerateTimelinePostsCommand
        {
            Request = new GenerateTimelinePostsRequest { TimelineId = timelineId, Provider = request.Provider, PostCount = request.PostCount }
        });
        return OkResponse(result, "AI đã sinh bài viết mẫu cho timeline.");
    }
}

public class GenerateTimelinePostsBodyDto
{
    public AiProvider Provider { get; set; } = AiProvider.Claude;
    public int? PostCount { get; set; }
}
