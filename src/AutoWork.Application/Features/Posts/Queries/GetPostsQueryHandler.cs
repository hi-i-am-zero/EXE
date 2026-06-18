using AutoMapper;
using AutoWork.Application.Common.Exceptions;
using AutoWork.Application.Common.Models;
using AutoWork.Application.DTOs.Posts;
using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Application.Interfaces.Services;
using MediatR;

namespace AutoWork.Application.Features.Posts.Queries;

public class GetPostsQueryHandler : IRequestHandler<GetPostsQuery, PaginatedList<PostDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public GetPostsQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<PaginatedList<PostDto>> Handle(GetPostsQuery request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is not Guid userId)
        {
            throw new UnauthorizedException("User is not authenticated.");
        }

        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var posts = await _unitOfWork.Posts.GetPagedAsync(userId, pageNumber, pageSize, request.Status, cancellationToken);
        var totalCount = await _unitOfWork.Posts.CountByUserIdAsync(userId, request.Status, cancellationToken);
        var dtos = _mapper.Map<IReadOnlyList<PostDto>>(posts);

        return PaginatedList<PostDto>.Create(dtos, totalCount, pageNumber, pageSize);
    }
}
