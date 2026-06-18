using AutoWork.Application.Common.Exceptions;
using AutoWork.Application.Common.Helpers;
using AutoWork.Application.DTOs.Users;
using AutoWork.Application.Features.Users.Queries;
using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Domain.Entities;
using AutoWork.Shared.Constants;
using AutoWork.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoWork.API.Controllers;

[Authorize]
public class UsersController : ApiControllerBase
{
    private readonly IMediator _mediator;
    private readonly IUnitOfWork _unitOfWork;

    public UsersController(IMediator mediator, IUnitOfWork unitOfWork)
    {
        _mediator = mediator;
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.Admin}")]
    public async Task<ActionResult<ApiResponse<PaginatedResult<UserDto>>>> GetUsers(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null)
    {
        var result = await _mediator.Send(new GetUsersQuery
        {
            PageNumber = page,
            PageSize = pageSize,
            Search = search
        });

        return OkResponse(PaginatedResult<UserDto>.Create(result.Items, result.TotalCount, result.PageNumber, result.PageSize));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetUser(Guid id)
    {
        var user = await _unitOfWork.Users.GetByIdWithRolesAsync(id)
            ?? throw new NotFoundException("User", id);

        return OkResponse(new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            AvatarUrl = user.AvatarUrl,
            Phone = user.Phone,
            IsActive = user.IsActive,
            LastLoginAt = user.LastLoginAt,
            ReferralCode = user.ReferralCode,
            CreatedAt = user.CreatedAt,
            Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList()
        });
    }

    [HttpPost]
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.Admin}")]
    public async Task<ActionResult<ApiResponse<UserDto>>> CreateUser([FromBody] CreateUserDto request)
    {
        if (await _unitOfWork.Users.EmailExistsAsync(request.Email))
            return FailResponse<UserDto>("Email already exists.");

        var user = new User
        {
            Email = request.Email.Trim().ToLowerInvariant(),
            PasswordHash = PasswordHelper.Hash(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Phone = request.Phone,
            IsActive = true,
            ReferralCode = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()
        };

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.Credits.AddAsync(new Credit { UserId = user.Id, Balance = PlanCredits.Free, TotalEarned = PlanCredits.Free });
        await _unitOfWork.SaveChangesAsync();

        return OkResponse(new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        }, "User created.");
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.Admin}")]
    public async Task<ActionResult<ApiResponse<UserDto>>> UpdateUser(Guid id, [FromBody] CreateUserDto request)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id)
            ?? throw new NotFoundException("User", id);

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.Phone = request.Phone;

        if (!string.IsNullOrWhiteSpace(request.Password))
            user.PasswordHash = PasswordHelper.Hash(request.Password);

        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return OkResponse(new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Phone = user.Phone,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        }, "User updated.");
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = AppRoles.SuperAdmin)]
    public async Task<ActionResult<ApiResponse>> DeleteUser(Guid id)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id)
            ?? throw new NotFoundException("User", id);

        await _unitOfWork.Users.DeleteAsync(user);
        await _unitOfWork.SaveChangesAsync();
        return OkResponse("User deleted.");
    }
}
