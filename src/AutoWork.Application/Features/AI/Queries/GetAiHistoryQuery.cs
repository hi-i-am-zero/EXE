using AutoWork.Application.Common.Models;
using AutoWork.Application.DTOs.AI;
using MediatR;

namespace AutoWork.Application.Features.AI.Queries;

public class GetAiHistoryQuery : IRequest<PaginatedList<AiHistoryDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
