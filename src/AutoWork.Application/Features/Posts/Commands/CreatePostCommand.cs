using AutoWork.Application.DTOs.Posts;
using MediatR;

namespace AutoWork.Application.Features.Posts.Commands;

public class CreatePostCommand : IRequest<PostDto>
{
    public CreatePostDto Request { get; set; } = null!;
}
