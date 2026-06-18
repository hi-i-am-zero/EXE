using AutoWork.Application.Common.Exceptions;
using AutoWork.Application.DTOs.Payments;
using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Application.Interfaces.Services;
using AutoWork.Domain.Entities;
using AutoWork.Domain.Enums;
using AutoWork.Infrastructure.Settings;
using AutoWork.Shared.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using DomainPaymentStatus = AutoWork.Domain.Enums.PaymentStatus;
using SharedCreditTransactionType = AutoWork.Shared.Enums.CreditTransactionType;

namespace AutoWork.Infrastructure.Payments;

public class PaymentService : IPaymentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICreditService _creditService;
    private readonly VNPayService _vnPayService;
    private readonly MoMoService _moMoService;
    private readonly ZaloPayService _zaloPayService;
    private readonly PaymentSettings _settings;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        IUnitOfWork unitOfWork,
        ICreditService creditService,
        VNPayService vnPayService,
        MoMoService moMoService,
        ZaloPayService zaloPayService,
        IOptions<PaymentSettings> settings,
        IHttpContextAccessor httpContextAccessor,
        ILogger<PaymentService> logger)
    {
        _unitOfWork = unitOfWork;
        _creditService = creditService;
        _vnPayService = vnPayService;
        _moMoService = moMoService;
        _zaloPayService = zaloPayService;
        _settings = settings.Value;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<PaymentDto> CreatePaymentAsync(Guid userId, CreatePaymentRequest request, CancellationToken cancellationToken = default)
    {
        var plan = await _unitOfWork.Plans.GetByIdAsync(request.PlanId, cancellationToken)
            ?? throw new NotFoundException("Plan", request.PlanId);

        if (!plan.IsActive)
        {
            throw new BadRequestException("Selected plan is not available.");
        }

        var transactionId = Guid.NewGuid().ToString("N")[..12];
        var returnUrl = string.IsNullOrWhiteSpace(request.ReturnUrl) ? _settings.DefaultReturnUrl : request.ReturnUrl;
        var cancelUrl = string.IsNullOrWhiteSpace(request.CancelUrl) ? _settings.DefaultCancelUrl : request.CancelUrl;
        var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "127.0.0.1";
        var notifyUrl = BuildWebhookUrl("momo");

        var invoice = new Invoice
        {
            UserId = userId,
            InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{transactionId}",
            Amount = plan.Price,
            Tax = 0,
            Total = plan.Price,
            Status = (int)InvoiceStatus.Pending,
            DueDate = DateTime.UtcNow.AddDays(3),
            Notes = $"Subscription: {plan.Name} (PlanId:{plan.Id})"
        };
        await _unitOfWork.Payments.AddInvoiceAsync(invoice, cancellationToken);

        var zaloAppTransId = $"{DateTime.UtcNow:yyMMdd}_{transactionId}";
        var storedTransactionId = request.Provider == PaymentProvider.ZaloPay ? zaloAppTransId : transactionId;

        var payment = new Payment
        {
            UserId = userId,
            InvoiceId = invoice.Id,
            Amount = plan.Price,
            PaymentMethod = (int)request.Provider,
            Status = (int)DomainPaymentStatus.Pending,
            TransactionId = storedTransactionId
        };
        await _unitOfWork.Payments.AddAsync(payment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var paymentUrl = request.Provider switch
        {
            PaymentProvider.VNPay => _vnPayService.CreatePaymentUrl(
                transactionId, plan.Price, invoice.Notes!, returnUrl, ipAddress, DateTime.UtcNow),
            PaymentProvider.MoMo => await _moMoService.CreatePaymentUrlAsync(
                transactionId, plan.Price, invoice.Notes!, returnUrl, notifyUrl, cancellationToken),
            PaymentProvider.ZaloPay => await _zaloPayService.CreatePaymentUrlAsync(
                zaloAppTransId, (long)plan.Price, invoice.Notes!, "{}", "[]", BuildWebhookUrl("zalopay"), cancellationToken),
            _ => throw new NotSupportedException($"Provider '{request.Provider}' is not supported.")
        };

        return new PaymentDto
        {
            Id = payment.Id,
            UserId = userId,
            InvoiceId = invoice.Id,
            PlanId = plan.Id,
            Provider = request.Provider,
            Status = payment.Status,
            Amount = plan.Price,
            TransactionId = storedTransactionId,
            PaymentUrl = paymentUrl,
            CreatedAt = payment.CreatedAt
        };
    }

    public async Task<PaymentDto> ProcessCallbackAsync(string provider, IDictionary<string, string> callbackData, CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<PaymentProvider>(provider, true, out var paymentProvider))
        {
            throw new BadRequestException($"Unknown payment provider: {provider}");
        }

        var isSuccessful = ValidateCallback(paymentProvider, callbackData, out var transactionId, out var failureReason);
        var payment = await _unitOfWork.Payments.GetByTransactionIdAsync(transactionId, cancellationToken)
            ?? throw new NotFoundException("Payment", transactionId);

        if (payment.Status == (int)DomainPaymentStatus.Completed)
        {
            return MapPayment(payment, paymentProvider);
        }

        if (!isSuccessful)
        {
            payment.Status = (int)DomainPaymentStatus.Failed;
            payment.FailureReason = failureReason;
            payment.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Payments.UpdateAsync(payment, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            throw new BadRequestException(failureReason ?? "Payment was not successful.");
        }

        payment.Status = (int)DomainPaymentStatus.Completed;
        payment.PaidAt = DateTime.UtcNow;
        payment.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Payments.UpdateAsync(payment, cancellationToken);

        var invoice = await _unitOfWork.Payments.GetInvoiceByIdAsync(payment.InvoiceId, cancellationToken);
        Guid? planId = null;
        if (invoice is not null)
        {
            invoice.Status = (int)InvoiceStatus.Paid;
            invoice.PaidAt = DateTime.UtcNow;
            invoice.UpdatedAt = DateTime.UtcNow;

            var plan = await ResolvePlanFromInvoiceAsync(invoice, cancellationToken);
            planId = plan?.Id;
            if (plan?.CreditsIncluded > 0)
            {
                await _creditService.AddCreditsAsync(
                    payment.UserId,
                    plan.CreditsIncluded,
                    SharedCreditTransactionType.Purchase,
                    $"Credits from plan: {plan.Name}",
                    cancellationToken);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Payment {TransactionId} completed via {Provider}", transactionId, paymentProvider);
        return MapPayment(payment, paymentProvider, planId);
    }

    public async Task<InvoiceDto?> GetInvoiceAsync(Guid invoiceId, CancellationToken cancellationToken = default)
    {
        var invoice = await _unitOfWork.Payments.GetInvoiceByIdAsync(invoiceId, cancellationToken);
        if (invoice is null)
        {
            return null;
        }

        return new InvoiceDto
        {
            Id = invoice.Id,
            UserId = invoice.UserId,
            InvoiceNumber = invoice.InvoiceNumber,
            Amount = invoice.Total,
            Currency = "VND",
            Status = invoice.Status,
            Description = invoice.Notes,
            PaidAt = invoice.PaidAt,
            CreatedAt = invoice.CreatedAt
        };
    }

    private bool ValidateCallback(
        PaymentProvider provider,
        IDictionary<string, string> callbackData,
        out string transactionId,
        out string? failureReason)
    {
        failureReason = null;
        transactionId = string.Empty;

        switch (provider)
        {
            case PaymentProvider.VNPay:
                if (!_vnPayService.ValidateCallback(callbackData, out transactionId, out var responseCode))
                {
                    failureReason = "Invalid VNPay signature.";
                    return false;
                }

                if (responseCode != "00")
                {
                    failureReason = $"VNPay declined payment (code {responseCode}).";
                    return false;
                }

                return true;

            case PaymentProvider.MoMo:
                if (!_moMoService.ValidateIpn(callbackData))
                {
                    failureReason = "Invalid MoMo signature.";
                    return false;
                }

                transactionId = callbackData.TryGetValue("orderId", out var orderId) ? orderId : string.Empty;
                if (string.IsNullOrWhiteSpace(transactionId))
                {
                    failureReason = "MoMo order id missing.";
                    return false;
                }

                if (callbackData.TryGetValue("resultCode", out var resultCode) && resultCode != "0")
                {
                    failureReason = $"MoMo declined payment (code {resultCode}).";
                    return false;
                }

                return true;

            case PaymentProvider.ZaloPay:
                if (!_zaloPayService.ValidateCallback(callbackData))
                {
                    failureReason = "Invalid ZaloPay signature.";
                    return false;
                }

                if (!callbackData.TryGetValue("data", out var dataPayload))
                {
                    failureReason = "ZaloPay callback data missing.";
                    return false;
                }

                using (var document = System.Text.Json.JsonDocument.Parse(dataPayload))
                {
                    if (document.RootElement.TryGetProperty("app_trans_id", out var appTransId))
                    {
                        transactionId = appTransId.GetString() ?? string.Empty;
                    }

                    if (document.RootElement.TryGetProperty("status", out var status) &&
                        status.GetInt32() != 1)
                    {
                        failureReason = "ZaloPay payment was not successful.";
                        return false;
                    }
                }

                if (string.IsNullOrWhiteSpace(transactionId))
                {
                    failureReason = "ZaloPay transaction id missing.";
                    return false;
                }

                return true;

            default:
                failureReason = $"Unsupported provider: {provider}";
                return false;
        }
    }

    private async Task<Plan?> ResolvePlanFromInvoiceAsync(Invoice invoice, CancellationToken cancellationToken)
    {
        if (invoice.Notes is null)
        {
            return null;
        }

        var planIdMarker = "PlanId:";
        var markerIndex = invoice.Notes.IndexOf(planIdMarker, StringComparison.OrdinalIgnoreCase);
        if (markerIndex >= 0)
        {
            var planIdValue = invoice.Notes[(markerIndex + planIdMarker.Length)..].Trim().TrimEnd(')');
            if (Guid.TryParse(planIdValue, out var planId))
            {
                return await _unitOfWork.Plans.GetByIdAsync(planId, cancellationToken);
            }
        }

        var plans = await _unitOfWork.Plans.GetActivePlansAsync(cancellationToken);
        return plans.FirstOrDefault(p => invoice.Notes.Contains(p.Name, StringComparison.OrdinalIgnoreCase));
    }

    private string BuildWebhookUrl(string provider)
    {
        if (Uri.TryCreate(_settings.DefaultReturnUrl, UriKind.Absolute, out var returnUri))
        {
            return $"{returnUri.Scheme}://{returnUri.Authority}/api/payments/webhooks/{provider}";
        }

        return $"/api/payments/webhooks/{provider}";
    }

    private static PaymentDto MapPayment(Payment payment, PaymentProvider provider, Guid? planId = null) =>
        new()
        {
            Id = payment.Id,
            UserId = payment.UserId,
            InvoiceId = payment.InvoiceId,
            PlanId = planId,
            Provider = provider,
            Status = payment.Status,
            Amount = payment.Amount,
            TransactionId = payment.TransactionId ?? string.Empty,
            CompletedAt = payment.PaidAt,
            CreatedAt = payment.CreatedAt
        };
}
