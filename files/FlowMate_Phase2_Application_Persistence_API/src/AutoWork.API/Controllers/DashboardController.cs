using AutoWork.Application.Interfaces.Services;
using AutoWork.Application.DTOs.Dashboard;
using AutoWork.Application.Features.Dashboard.Queries;
using AutoWork.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoWork.API.Controllers;

[Authorize]
[Route("api/dashboard")]
public class DashboardController : ApiControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;

    public DashboardController(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    [HttpGet("stats")]
    public async Task<ActionResult<ApiResponse<DashboardStatsDto>>> GetStats() =>
        OkResponse(await _mediator.Send(new GetDashboardStatsQuery()));
}
