using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Domain.Entities;
using AutoWork.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace AutoWork.Persistence.Repositories;

public class PaymentRepository : Repository<Payment>, IPaymentRepository
{
    public PaymentRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Payment>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        await DbSet
            .AsNoTracking()
            .Include(p => p.Invoice)
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<Payment?> GetByTransactionIdAsync(
        string transactionId,
        CancellationToken cancellationToken = default) =>
        await DbSet
            .Include(p => p.Invoice)
            .FirstOrDefaultAsync(p => p.TransactionId == transactionId, cancellationToken);

    public async Task<Invoice?> GetInvoiceByIdAsync(Guid invoiceId, CancellationToken cancellationToken = default) =>
        await Context.Invoices
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.Id == invoiceId, cancellationToken);

    public async Task<Invoice> AddInvoiceAsync(Invoice invoice, CancellationToken cancellationToken = default)
    {
        await Context.Invoices.AddAsync(invoice, cancellationToken);
        return invoice;
    }
}
