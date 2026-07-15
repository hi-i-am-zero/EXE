using AutoWork.Application.DTOs.Channels;
using AutoWork.Application.Features.Channels.Commands;
using AutoWork.Application.Features.Channels.Queries;
using AutoWork.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoWork.API.Controllers;

/// <summary>
/// FlowMate v2: quản lý ChannelAccount (kênh đã liên kết) dùng chung cho Brand Memory,
/// Campaign, và Post đa nền tảng. Endpoint "connect" ở đây chỉ tạo liên kết THỦ CÔNG
/// (chưa có AccessToken thật) — OAuth Facebook/Zalo thật sẽ làm ở giai đoạn sau.
/// </summary>
[Authorize]
[Route("api/channel-accounts")]
public class ChannelAccountsController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public ChannelAccountsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("channels")]
    public async Task<ActionResult<ApiResponse<List<ChannelDto>>>> GetChannels()
    {
        var result = await _mediator.Send(new GetChannelsQuery());
        return OkResponse(result);
    }

    [HttpGet("projects/{projectId:guid}")]
    public async Task<ActionResult<ApiResponse<List<ChannelAccountDto>>>> GetChannelAccounts(Guid projectId)
    {
        var result = await _mediator.Send(new GetChannelAccountsQuery { ProjectId = projectId });
        return OkResponse(result);
    }

    [HttpPost("projects/{projectId:guid}")]
    public async Task<ActionResult<ApiResponse<ChannelAccountDto>>> CreateChannelAccount(
        Guid projectId, [FromBody] CreateChannelAccountDto request)
    {
        request.ProjectId = projectId;
        var result = await _mediator.Send(new CreateChannelAccountCommand { Request = request });
        return OkResponse(result, "Đã thêm kênh bán hàng.");
    }
}
