using AutoMapper;
using AutoWork.Application.Common.Exceptions;
using AutoWork.Application.Common.Models;
using AutoWork.Application.DTOs.AI;
using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Application.Interfaces.Services;
using MediatR;

namespace AutoWork.Application.Features.AI.Queries;

public class GetAiHistoryQueryHandler : IRequestHandler<GetAiHistoryQuery, PaginatedList<AiHistoryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public GetAiHistoryQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<PaginatedList<AiHistoryDto>> Handle(GetAiHistoryQuery request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is not Guid userId)
        {
            throw new UnauthorizedException("User is not authenticated.");
        }

        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var items = await _unitOfWork.Ai.GetHistoryByUserIdAsync(userId, pageNumber, pageSize, cancellationToken);
        var totalCount = await _unitOfWork.Ai.CountHistoryByUserIdAsync(userId, cancellationToken);
        var dtos = _mapper.Map<IReadOnlyList<AiHistoryDto>>(items);

        return PaginatedList<AiHistoryDto>.Create(dtos, totalCount, pageNumber, pageSize);
    }
}
