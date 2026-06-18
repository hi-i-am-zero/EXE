namespace AutoWork.Application.Interfaces.Repositories;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IPostRepository Posts { get; }
    IPlanRepository Plans { get; }
    ICreditRepository Credits { get; }
    IAiRepository Ai { get; }
    IFacebookRepository Facebook { get; }
    IWordPressRepository WordPress { get; }
    IZaloRepository Zalo { get; }
    IScheduleRepository Schedules { get; }
    IAffiliateRepository Affiliates { get; }
    IPaymentRepository Payments { get; }
    INotificationRepository Notifications { get; }
    IMediaRepository Media { get; }
    IProjectRepository Projects { get; }
    IAuditLogRepository AuditLogs { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
