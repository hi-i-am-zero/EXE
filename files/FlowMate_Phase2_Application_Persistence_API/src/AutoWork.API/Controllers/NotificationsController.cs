using AutoWork.Application.DTOs.Notifications;
using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Application.Interfaces.Services;
using AutoWork.Shared.Enums;
using AutoWork.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoWork.API.Controllers;

[Authorize]
public class NotificationsController : ApiControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public NotificationsController(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResult<NotificationDto>>>> GetNotifications(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] bool? isRead = null)
    {
        var userId = _currentUser.UserId!.Value;
        var items = await _unitOfWork.Notifications.GetByUserIdAsync(userId, page, pageSize, isRead);
        var total = await _unitOfWork.Notifications.CountByUserIdAsync(userId, isRead);

        var dtos = items.Select(n => new NotificationDto
        {
            Id = n.Id,
            Title = n.Title,
            Message = n.Message,
            Type = (NotificationType)n.Type,
            IsRead = n.IsRead,
            Link = n.ReferenceType is not null && n.ReferenceId.HasValue
                ? $"/{n.ReferenceType.ToLowerInvariant()}/{n.ReferenceId}"
                : null,
            CreatedAt = n.CreatedAt
        }).ToList();

        return OkResponse(PaginatedResult<NotificationDto>.Create(dtos, total, page, pageSize));
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<ApiResponse<int>>> GetUnreadCount() =>
        OkResponse(await _unitOfWork.Notifications.CountUnreadByUserIdAsync(_currentUser.UserId!.Value));

    [HttpPut("{id:guid}/read")]
    public async Task<ActionResult<ApiResponse>> MarkAsRead(Guid id)
    {
        await _unitOfWork.Notifications.MarkAsReadAsync(id);
        await _unitOfWork.SaveChangesAsync();
        return OkResponse("Notification marked as read.");
    }

    [HttpPut("read-all")]
    public async Task<ActionResult<ApiResponse>> MarkAllAsRead()
    {
        await _unitOfWork.Notifications.MarkAllAsReadAsync(_currentUser.UserId!.Value);
        await _unitOfWork.SaveChangesAsync();
        return OkResponse("All notifications marked as read.");
    }
}
