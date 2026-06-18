using AutoMapper;
using AutoWork.Application.Common.Exceptions;
using AutoWork.Application.Common.Models;
using AutoWork.Application.DTOs.Users;
using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Application.Interfaces.Services;
using MediatR;

namespace AutoWork.Application.Features.Users.Queries;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, PaginatedList<UserDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public GetUsersQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<PaginatedList<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsInRole("Admin") && !_currentUserService.IsInRole("SuperAdmin"))
        {
            throw new UnauthorizedException("You do not have permission to view users.");
        }

        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var users = await _unitOfWork.Users.GetPagedAsync(pageNumber, pageSize, request.Search, cancellationToken);
        var totalCount = await _unitOfWork.Users.CountAsync(request.Search, cancellationToken);
        var dtos = _mapper.Map<IReadOnlyList<UserDto>>(users);

        return PaginatedList<UserDto>.Create(dtos, totalCount, pageNumber, pageSize);
    }
}
