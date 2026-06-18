using AutoWork.Application.DTOs.Payments;

namespace AutoWork.Application.Interfaces.Services;

public interface IPaymentService
{
    Task<PaymentDto> CreatePaymentAsync(Guid userId, CreatePaymentRequest request, CancellationToken cancellationToken = default);
    Task<PaymentDto> ProcessCallbackAsync(string provider, IDictionary<string, string> callbackData, CancellationToken cancellationToken = default);
    Task<InvoiceDto?> GetInvoiceAsync(Guid invoiceId, CancellationToken cancellationToken = default);
}
