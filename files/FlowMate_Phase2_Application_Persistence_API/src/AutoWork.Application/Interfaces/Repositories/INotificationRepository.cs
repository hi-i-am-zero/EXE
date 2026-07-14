using AutoWork.Domain.Entities;

namespace AutoWork.Application.Interfaces.Repositories;

public interface INotificationRepository : IRepository<Notification>
{
    Task<IReadOnlyList<Notification>> GetByUserIdAsync(Guid userId, int pageNumber, int pageSize, bool? isRead = null, CancellationToken cancellationToken = default);
    Task<int> CountByUserIdAsync(Guid userId, bool? isRead = null, CancellationToken cancellationToken = default);
    Task<int> CountUnreadByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default);
    Task MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default);
}
