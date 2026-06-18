using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Persistence.Context;
using Microsoft.EntityFrameworkCore.Storage;

namespace AutoWork.Persistence.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(
        ApplicationDbContext context,
        IUserRepository users,
        IPostRepository posts,
        IPlanRepository plans,
        ICreditRepository credits,
        IAiRepository ai,
        IFacebookRepository facebook,
        IWordPressRepository wordPress,
        IZaloRepository zalo,
        IScheduleRepository schedules,
        IAffiliateRepository affiliates,
        IPaymentRepository payments,
        INotificationRepository notifications,
        IMediaRepository media,
        IProjectRepository projects,
        IAuditLogRepository auditLogs)
    {
        _context = context;
        Users = users;
        Posts = posts;
        Plans = plans;
        Credits = credits;
        Ai = ai;
        Facebook = facebook;
        WordPress = wordPress;
        Zalo = zalo;
        Schedules = schedules;
        Affiliates = affiliates;
        Payments = payments;
        Notifications = notifications;
        Media = media;
        Projects = projects;
        AuditLogs = auditLogs;
    }

    public IUserRepository Users { get; }
    public IPostRepository Posts { get; }
    public IPlanRepository Plans { get; }
    public ICreditRepository Credits { get; }
    public IAiRepository Ai { get; }
    public IFacebookRepository Facebook { get; }
    public IWordPressRepository WordPress { get; }
    public IZaloRepository Zalo { get; }
    public IScheduleRepository Schedules { get; }
    public IAffiliateRepository Affiliates { get; }
    public IPaymentRepository Payments { get; }
    public INotificationRepository Notifications { get; }
    public IMediaRepository Media { get; }
    public IProjectRepository Projects { get; }
    public IAuditLogRepository AuditLogs { get; }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await _context.SaveChangesAsync(cancellationToken);

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default) =>
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is not null)
        {
            await _transaction.CommitAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is not null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
