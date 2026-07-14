using AutoWork.Application.DTOs.Facebook;
using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Application.Interfaces.Services;
using AutoWork.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoWork.API.Controllers;

[Authorize]
[Route("api/facebook")]
public class FacebookController : ApiControllerBase
{
    private readonly IFacebookService _facebookService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public FacebookController(IFacebookService facebookService, IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _facebookService = facebookService;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    [HttpGet("connect")]
    public async Task<ActionResult<ApiResponse<object>>> Connect([FromQuery] string redirectUri) =>
        OkResponse<object>(new { url = await _facebookService.GetAuthorizationUrlAsync(_currentUser.UserId!.Value, redirectUri) });

    [HttpPost("connect")]
    public async Task<ActionResult<ApiResponse<FacebookAccountDto>>> ConnectCallback(
        [FromBody] FacebookConnectRequest request) =>
        OkResponse(await _facebookService.ConnectAccountAsync(
            _currentUser.UserId!.Value, request.Code, request.RedirectUri));

    [HttpGet("pages")]
    public async Task<ActionResult<ApiResponse<List<FacebookPageDto>>>> GetPages()
    {
        var pages = await _unitOfWork.Facebook.GetPagesByUserIdAsync(_currentUser.UserId!.Value);
        var dtos = pages.Select(p => new FacebookPageDto
        {
            Id = p.Id,
            PageId = p.PageId,
            PageName = p.Name,
            Category = p.Category,
            PictureUrl = p.ProfilePictureUrl,
            IsActive = p.IsConnected
        }).ToList();

        return OkResponse(dtos);
    }

    [HttpPost("pages/{accountId:guid}/sync")]
    public async Task<ActionResult<ApiResponse<List<FacebookPageDto>>>> SyncPages(Guid accountId) =>
        OkResponse((await _facebookService.SyncPagesAsync(_currentUser.UserId!.Value, accountId)).ToList());

    [HttpPost("publish")]
    public async Task<ActionResult<ApiResponse<object>>> Publish([FromBody] PublishFacebookPostDto request) =>
        OkResponse<object>(new { externalId = await _facebookService.PublishPostAsync(_currentUser.UserId!.Value, request) });
}

public class FacebookConnectRequest
{
    public string Code { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = string.Empty;
}
