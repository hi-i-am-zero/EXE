using AutoWork.Application.Common.Models;
using AutoWork.Application.DTOs.Posts;
using MediatR;

namespace AutoWork.Application.Features.Posts.Queries;

public class GetPostsQuery : IRequest<PaginatedList<PostDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int? Status { get; set; }
}
