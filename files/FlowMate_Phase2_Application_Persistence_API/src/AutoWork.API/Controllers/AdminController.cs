using AutoWork.Application.DTOs.Admin;
using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Shared.Constants;
using AutoWork.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentStatus = AutoWork.Shared.Enums.PaymentStatus;

namespace AutoWork.API.Controllers;

[Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.Admin}")]
[Route("api/admin")]
public class AdminController : ApiControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public AdminController(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    [HttpGet("dashboard")]
    public async Task<ActionResult<ApiResponse<AdminDashboardDto>>> GetDashboard()
    {
        var users = await _unitOfWork.Users.GetAllAsync();
        var posts = await _unitOfWork.Posts.GetAllAsync();
        var payments = await _unitOfWork.Payments.GetAllAsync();

        return OkResponse(new AdminDashboardDto
        {
            TotalUsers = users.Count,
            TotalPosts = posts.Count,
            TotalPayments = payments.Count,
            TotalRevenue = payments.Where(p => p.Status == (int)PaymentStatus.Completed).Sum(p => p.Amount)
        });
    }

    [HttpPut("users/{id:guid}/status")]
    public async Task<ActionResult<ApiResponse>> UpdateUserStatus(Guid id, [FromBody] UpdateUserStatusRequest request)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id)
            ?? throw new Application.Common.Exceptions.NotFoundException("User", id);

        user.IsActive = request.IsActive;
        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();
        return OkResponse("User status updated.");
    }
}
