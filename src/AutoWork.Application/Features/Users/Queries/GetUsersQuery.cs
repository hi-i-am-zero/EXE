using AutoWork.Application.Common.Models;
using AutoWork.Application.DTOs.Users;
using MediatR;

namespace AutoWork.Application.Features.Users.Queries;

public class GetUsersQuery : IRequest<PaginatedList<UserDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Search { get; set; }
}
