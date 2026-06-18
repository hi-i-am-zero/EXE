using AutoWork.Application.DTOs.Posts;
using AutoWork.Application.Features.Posts.Commands;
using AutoWork.Application.Features.Posts.Queries;
using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Application.Interfaces.Services;
using AutoWork.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoWork.API.Controllers;

[Authorize]
public class PostsController : ApiControllerBase
{
    private readonly IMediator _mediator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public PostsController(IMediator mediator, IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResult<PostDto>>>> GetPosts(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] int? status = null)
    {
        var result = await _mediator.Send(new GetPostsQuery { PageNumber = page, PageSize = pageSize, Status = status });
        return OkResponse(PaginatedResult<PostDto>.Create(result.Items, result.TotalCount, result.PageNumber, result.PageSize));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<PostDto>>> GetPost(Guid id)
    {
        var post = await _unitOfWork.Posts.GetByIdWithDetailsAsync(id)
            ?? throw new Application.Common.Exceptions.NotFoundException("Post", id);
        return OkResponse(MapPost(post));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<PostDto>>> CreatePost([FromBody] CreatePostDto request) =>
        OkResponse(await _mediator.Send(new CreatePostCommand { Request = request }), "Post created.");

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<PostDto>>> UpdatePost(Guid id, [FromBody] CreatePostDto request)
    {
        var post = await _unitOfWork.Posts.GetByIdWithDetailsAsync(id)
            ?? throw new Application.Common.Exceptions.NotFoundException("Post", id);

        post.Title = request.Title;
        if (post.Contents.Any())
            post.Contents.First().Content = request.Content;
        else
            post.Contents.Add(new Domain.Entities.PostContent { PostId = post.Id, Content = request.Content, ContentType = request.ContentType });

        await _unitOfWork.Posts.UpdateAsync(post);
        await _unitOfWork.SaveChangesAsync();
        return OkResponse(MapPost(post), "Post updated.");
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse>> DeletePost(Guid id)
    {
        var post = await _unitOfWork.Posts.GetByIdAsync(id)
            ?? throw new Application.Common.Exceptions.NotFoundException("Post", id);
        await _unitOfWork.Posts.DeleteAsync(post);
        await _unitOfWork.SaveChangesAsync();
        return OkResponse("Post deleted.");
    }

    private static PostDto MapPost(Domain.Entities.Post post) => new()
    {
        Id = post.Id,
        ProjectId = post.ProjectId,
        ChannelAccountId = post.ChannelAccountId,
        Title = post.Title,
        Status = post.Status,
        ExternalPostId = post.ExternalPostId,
        PublishedUrl = post.PublishedUrl,
        PublishedAt = post.PublishedAt,
        CreatedAt = post.CreatedAt
    };
}
