using AutoWork.Application.DTOs.Campaigns;
using AutoWork.Application.Features.Campaigns.Commands;
using AutoWork.Application.Features.Campaigns.Queries;
using AutoWork.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoWork.API.Controllers;

/// <summary>FlowMate v2: hoàn toàn mới, không có ở bản AutoWork cũ.</summary>
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

    [HttpPost("projects/{projectId:guid}")]
    public async Task<ActionResult<ApiResponse<CampaignDto>>> CreateCampaign(
        Guid projectId, [FromBody] CreateCampaignDto request)
    {
        request.ProjectId = projectId;
        var result = await _mediator.Send(new CreateCampaignCommand { Request = request });
        return OkResponse(result, "Đã tạo chiến dịch.");
    }
}
