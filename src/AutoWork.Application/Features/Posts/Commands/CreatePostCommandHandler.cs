using AutoMapper;
using AutoWork.Application.Common.Exceptions;
using AutoWork.Application.DTOs.Posts;
using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Application.Interfaces.Services;
using AutoWork.Domain.Entities;
using MediatR;

namespace AutoWork.Application.Features.Posts.Commands;

public class CreatePostCommandHandler : IRequestHandler<CreatePostCommand, PostDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public CreatePostCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<PostDto> Handle(CreatePostCommand command, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is not Guid userId)
        {
            throw new UnauthorizedException("User is not authenticated.");
        }

        var project = await _unitOfWork.Projects.GetByIdAsync(command.Request.ProjectId, cancellationToken);
        if (project is null || project.UserId != userId)
        {
            throw new NotFoundException(nameof(Project), command.Request.ProjectId);
        }

        var post = new Post
        {
            ProjectId = command.Request.ProjectId,
            ChannelAccountId = command.Request.ChannelAccountId,
            Title = command.Request.Title.Trim(),
            Status = 1,
            Contents =
            [
                new PostContent
                {
                    ContentType = command.Request.ContentType,
                    Content = command.Request.Content,
                    SortOrder = 0,
                    MediaFileId = command.Request.MediaFileId
                }
            ]
        };

        await _unitOfWork.Posts.AddAsync(post, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var created = await _unitOfWork.Posts.GetByIdWithDetailsAsync(post.Id, cancellationToken)
            ?? post;

        return _mapper.Map<PostDto>(created);
    }
}
