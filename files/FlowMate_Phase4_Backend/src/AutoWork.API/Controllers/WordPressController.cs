using AutoWork.Application.DTOs.WordPress;
using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Application.Interfaces.Services;
using AutoWork.Domain.Entities;
using AutoWork.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoWork.API.Controllers;

[Authorize]
[Route("api/wordpress")]
public class WordPressController : ApiControllerBase
{
    private readonly IWordPressService _wordPressService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public WordPressController(IWordPressService wordPressService, IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _wordPressService = wordPressService;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    [HttpGet("sites")]
    public async Task<ActionResult<ApiResponse<List<WordPressSiteDto>>>> GetSites()
    {
        var sites = await _unitOfWork.WordPress.GetSitesByUserIdAsync(_currentUser.UserId!.Value);
        return OkResponse(sites.Select(MapSite).ToList());
    }

    [HttpPost("sites")]
    public async Task<ActionResult<ApiResponse<WordPressSiteDto>>> AddSite([FromBody] WordPressSiteDto request)
    {
        var connected = await _wordPressService.TestConnectionAsync(request);
        var site = new WordPressSite
        {
            UserId = _currentUser.UserId!.Value,
            SiteName = request.SiteName,
            SiteUrl = request.SiteUrl,
            Username = request.Username,
            ApplicationPassword = request.ApplicationPassword,
            IsWooCommerce = request.IsWooCommerce,
            IsConnected = connected
        };

        await _unitOfWork.WordPress.AddAsync(site);
        await _unitOfWork.SaveChangesAsync();
        return OkResponse(MapSite(site), connected ? "Site connected." : "Site saved but connection test failed.");
    }

    [HttpPut("sites/{id:guid}")]
    public async Task<ActionResult<ApiResponse<WordPressSiteDto>>> UpdateSite(Guid id, [FromBody] WordPressSiteDto request)
    {
        var site = await _unitOfWork.WordPress.GetByIdAsync(id)
            ?? throw new Application.Common.Exceptions.NotFoundException("Site", id);

        if (site.UserId != _currentUser.UserId)
            throw new UnauthorizedAccessException();

        site.SiteName = request.SiteName;
        site.SiteUrl = request.SiteUrl;
        site.Username = request.Username;
        site.ApplicationPassword = request.ApplicationPassword;
        site.IsWooCommerce = request.IsWooCommerce;
        site.IsConnected = await _wordPressService.TestConnectionAsync(request);

        await _unitOfWork.WordPress.UpdateAsync(site);
        await _unitOfWork.SaveChangesAsync();
        return OkResponse(MapSite(site));
    }

    [HttpDelete("sites/{id:guid}")]
    public async Task<ActionResult<ApiResponse>> DeleteSite(Guid id)
    {
        var site = await _unitOfWork.WordPress.GetByIdAsync(id)
            ?? throw new Application.Common.Exceptions.NotFoundException("Site", id);

        if (site.UserId != _currentUser.UserId)
            throw new UnauthorizedAccessException();

        await _unitOfWork.WordPress.DeleteAsync(site);
        await _unitOfWork.SaveChangesAsync();
        return OkResponse("Site removed.");
    }

    [HttpGet("sites/{siteId:guid}/posts")]
    public async Task<ActionResult<ApiResponse<List<WordPressPostDto>>>> GetPosts(Guid siteId)
    {
        var site = await _unitOfWork.WordPress.GetSiteWithPostsAsync(siteId)
            ?? throw new Application.Common.Exceptions.NotFoundException("Site", siteId);

        if (site.UserId != _currentUser.UserId)
            throw new UnauthorizedAccessException();

        var posts = site.Posts.Select(p => new WordPressPostDto
        {
            Id = p.Id,
            Title = p.Title,
            Content = p.Excerpt ?? string.Empty,
            Status = p.Status.ToString(),
            ExternalPostId = p.ExternalPostId,
            Slug = p.Permalink
        }).ToList();

        return OkResponse(posts);
    }

    [HttpPost("posts")]
    public async Task<ActionResult<ApiResponse<object>>> PublishPost([FromBody] CreateWordPressPostDto request) =>
        OkResponse<object>(new { url = await _wordPressService.PublishPostAsync(_currentUser.UserId!.Value, request) });

    private static WordPressSiteDto MapSite(WordPressSite site) => new()
    {
        Id = site.Id,
        SiteName = site.SiteName,
        SiteUrl = site.SiteUrl,
        Username = site.Username,
        ApplicationPassword = site.ApplicationPassword,
        IsWooCommerce = site.IsWooCommerce,
        IsActive = site.IsConnected
    };
}
