using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace AutoWork.Infrastructure.SignalR;

[Authorize]
public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrWhiteSpace(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrWhiteSpace(userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    public Task MarkAsRead(Guid notificationId) =>
        Clients.Caller.SendAsync("NotificationRead", notificationId);
}

public static class NotificationHubClient
{
    public const string HubPath = "/hubs/notifications";

    public static Task SendToUserAsync(
        IHubContext<NotificationHub> hubContext,
        Guid userId,
        string title,
        string message,
        string? link = null) =>
        hubContext.Clients.Group(userId.ToString()).SendAsync("ReceiveNotification", new
        {
            title,
            message,
            link,
            createdAt = DateTime.UtcNow
        });
}
