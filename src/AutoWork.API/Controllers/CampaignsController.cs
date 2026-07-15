using AutoWork.Application.DTOs.AI;
using AutoWork.Application.DTOs.Campaigns;
using AutoWork.Application.DTOs.Timelines;
using AutoWork.Application.Features.Campaigns.Commands;
using AutoWork.Application.Features.Campaigns.Queries;
using AutoWork.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoWork.API.Controllers;

/// <summary>FlowMate v2: hoàn toàn mới, không có ở bản AutoWork cũ.
/// Toàn bộ luồng 5 bước "Chiến dịch tự động": (1) CreateCampaign — thông tin chiến dịch,
/// (2) GenerateProductDescription — ảnh sản phẩm + AI mô tả chung, (3) GenerateTimelineOptions —
/// AI sinh 3 phương án timeline, (4) ConfirmTimelineOption — chọn 1 phương án, thật sự tạo bài viết,
/// (5) TimelinesController.ScheduleTimeline — "Đăng bài", lên lịch cả timeline.</summary>
[Authorize]
[Route("api/campaigns")]
public class CampaignsController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public CampaignsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("lookup-options")]
    public async Task<ActionResult<ApiResponse<CampaignLookupOptionsDto>>> GetLookupOptions()
    {
        var result = await _mediator.Send(new GetCampaignLookupOptionsQuery());
        return OkResponse(result);
    }

    [HttpGet("projects/{projectId:guid}")]
    public async Task<ActionResult<ApiResponse<List<CampaignDto>>>> GetCampaigns(Guid projectId)
    {
        var result = await _mediator.Send(new GetCampaignsQuery { ProjectId = projectId });
        return OkResponse(result);
    }

    /// <summary>Bước 1: thông tin chiến dịch (mục tiêu, ưu đãi, ngày, số bài đăng).</summary>
    [HttpPost("projects/{projectId:guid}")]
    public async Task<ActionResult<ApiResponse<CampaignDto>>> CreateCampaign(
        Guid projectId, [FromBody] CreateCampaignDto request)
    {
        request.ProjectId = projectId;
        var result = await _mediator.Send(new CreateCampaignCommand { Request = request });
        return OkResponse(result, "Đã tạo chiến dịch.");
    }

    /// <summary>Bước 2: upload ảnh sản phẩm, AI sinh mô tả chung để làm ngữ cảnh cho bước 3.</summary>
    [HttpPost("{campaignId:guid}/product-description")]
    public async Task<ActionResult<ApiResponse<GenerateCampaignProductDescriptionResponse>>> GenerateProductDescription(
        Guid campaignId, [FromBody] GenerateCampaignProductDescriptionBodyDto request)
    {
        var result = await _mediator.Send(new GenerateCampaignProductDescriptionCommand
        {
            Request = new GenerateCampaignProductDescriptionRequest
            {
                CampaignId = campaignId,
                MediaFileIds = request.MediaFileIds,
                Provider = request.Provider
            }
        });
        return OkResponse(result, "AI đã sinh mô tả sản phẩm.");
    }

    /// <summary>Bước 3: AI sinh 3 phương án timeline (Classic/Roadmap/Calendar) để người dùng chọn 1.
    /// KHÔNG lưu DB ở bước này — chỉ trả về để hiển thị.</summary>
    [HttpPost("{campaignId:guid}/timeline-options")]
    public async Task<ActionResult<ApiResponse<GenerateTimelineOptionsResponse>>> GenerateTimelineOptions(
        Guid campaignId, [FromBody] GenerateTimelineOptionsBodyDto request)
    {
        var result = await _mediator.Send(new GenerateTimelineOptionsCommand
        {
            Request = new GenerateTimelineOptionsRequest { CampaignId = campaignId, Provider = request.Provider }
        });
        return OkResponse(result, "AI đã đề xuất 3 phương án timeline.");
    }

    /// <summary>Bước 4: xác nhận phương án đã chọn — thật sự tạo Timeline + Post + PostContent + Hashtag.</summary>
    [HttpPost("{campaignId:guid}/confirm-timeline")]
    public async Task<ActionResult<ApiResponse<TimelineDto>>> ConfirmTimelineOption(
        Guid campaignId, [FromBody] ConfirmTimelineOptionBodyDto request)
    {
        var result = await _mediator.Send(new ConfirmTimelineOptionCommand
        {
            Request = new ConfirmTimelineOptionRequest
            {
                CampaignId = campaignId,
                TemplateTypeId = request.TemplateTypeId,
                TimelineName = request.TimelineName,
                Posts = request.Posts
            }
        });
        return OkResponse(result, "Đã tạo timeline và các bài viết.");
    }
}

public class GenerateCampaignProductDescriptionBodyDto
{
    public List<Guid> MediaFileIds { get; set; } = [];
    public AutoWork.Shared.Enums.AiProvider Provider { get; set; } = AutoWork.Shared.Enums.AiProvider.Claude;
}

public class GenerateTimelineOptionsBodyDto
{
    public AutoWork.Shared.Enums.AiProvider Provider { get; set; } = AutoWork.Shared.Enums.AiProvider.Claude;
}

public class ConfirmTimelineOptionBodyDto
{
    public Guid TemplateTypeId { get; set; }
    public string TimelineName { get; set; } = string.Empty;
    public List<DraftPostItemDto> Posts { get; set; } = [];
}
