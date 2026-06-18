using AutoWork.Domain.Entities;

namespace AutoWork.Application.Interfaces.Repositories;

public interface IPaymentRepository : IRepository<Payment>
{
    Task<IReadOnlyList<Payment>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Payment?> GetByTransactionIdAsync(string transactionId, CancellationToken cancellationToken = default);
    Task<Invoice?> GetInvoiceByIdAsync(Guid invoiceId, CancellationToken cancellationToken = default);
    Task<Invoice> AddInvoiceAsync(Invoice invoice, CancellationToken cancellationToken = default);
}
