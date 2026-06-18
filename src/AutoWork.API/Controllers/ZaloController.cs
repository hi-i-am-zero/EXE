using AutoWork.Application.DTOs.Zalo;
using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Application.Interfaces.Services;
using AutoWork.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoWork.API.Controllers;

[Authorize]
[Route("api/zalo")]
public class ZaloController : ApiControllerBase
{
    private readonly IZaloService _zaloService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public ZaloController(IZaloService zaloService, IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _zaloService = zaloService;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    [HttpGet("connect")]
    public async Task<ActionResult<ApiResponse<object>>> Connect([FromQuery] string redirectUri) =>
        OkResponse<object>(new { url = await _zaloService.GetAuthorizationUrlAsync(_currentUser.UserId!.Value, redirectUri) });

    [HttpPost("accounts")]
    public async Task<ActionResult<ApiResponse<ZaloAccountDto>>> ConnectAccount([FromBody] ZaloConnectRequest request) =>
        OkResponse(await _zaloService.ConnectAccountAsync(_currentUser.UserId!.Value, request.Code));

    [HttpGet("accounts")]
    public async Task<ActionResult<ApiResponse<List<ZaloAccountDto>>>> GetAccounts()
    {
        var accounts = await _unitOfWork.Zalo.GetAccountsByUserIdAsync(_currentUser.UserId!.Value);
        return OkResponse(accounts.Select(a => new ZaloAccountDto
        {
            Id = a.Id,
            OaId = a.ZaloUserId,
            OaName = a.DisplayName,
            IsActive = a.IsConnected,
            TokenExpiresAt = a.TokenExpiresAt
        }).ToList());
    }

    [HttpGet("accounts/{accountId:guid}/posts")]
    public async Task<ActionResult<ApiResponse<List<ZaloPostDto>>>> GetPosts(Guid accountId)
    {
        var account = await _unitOfWork.Zalo.GetAccountWithPostsAsync(accountId)
            ?? throw new Application.Common.Exceptions.NotFoundException("Account", accountId);

        if (account.UserId != _currentUser.UserId)
            throw new UnauthorizedAccessException();

        var posts = account.Posts.Select(p => new ZaloPostDto
        {
            Id = p.Id,
            ZaloAccountId = p.ZaloAccountId,
            Content = p.Content,
            Status = p.Status.ToString(),
            ExternalPostId = p.ExternalPostId,
            CreatedAt = p.CreatedAt
        }).ToList();

        return OkResponse(posts);
    }

    [HttpPost("posts")]
    public async Task<ActionResult<ApiResponse<object>>> PublishPost([FromBody] PublishZaloPostRequest request) =>
        OkResponse<object>(new
        {
            externalId = await _zaloService.PublishPostAsync(_currentUser.UserId!.Value, new ZaloPostDto
            {
                ZaloAccountId = request.ZaloAccountId,
                Title = request.Title,
                Content = request.Content,
                MediaUrl = request.MediaUrl,
                ScheduledAt = request.ScheduledAt
            })
        });
}

public class ZaloConnectRequest
{
    public string Code { get; set; } = string.Empty;
}
