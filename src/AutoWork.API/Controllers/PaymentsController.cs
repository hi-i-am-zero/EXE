using AutoWork.Application.DTOs.Payments;
using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Application.Interfaces.Services;
using AutoWork.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoWork.API.Controllers;

[Authorize]
[Route("api/payments")]
public class PaymentsController : ApiControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public PaymentsController(IPaymentService paymentService, IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _paymentService = paymentService;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<PaymentDto>>>> GetPayments()
    {
        var payments = await _unitOfWork.Payments.GetByUserIdAsync(_currentUser.UserId!.Value);
        return OkResponse(payments.Select(p => new PaymentDto
        {
            Id = p.Id,
            UserId = p.UserId,
            InvoiceId = p.InvoiceId,
            Provider = (Shared.Enums.PaymentProvider)p.PaymentMethod,
            Status = p.Status,
            Amount = p.Amount,
            TransactionId = p.TransactionId ?? string.Empty,
            CompletedAt = p.PaidAt,
            CreatedAt = p.CreatedAt
        }).ToList());
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<PaymentDto>>> CreatePayment([FromBody] CreatePaymentRequest request) =>
        OkResponse(await _paymentService.CreatePaymentAsync(_currentUser.UserId!.Value, request));

    [HttpGet("invoices/{invoiceId:guid}")]
    public async Task<ActionResult<ApiResponse<InvoiceDto>>> GetInvoice(Guid invoiceId)
    {
        var invoice = await _paymentService.GetInvoiceAsync(invoiceId)
            ?? throw new Application.Common.Exceptions.NotFoundException("Invoice", invoiceId);
        return OkResponse(invoice);
    }

    [HttpPost("webhooks/vnpay")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<PaymentDto>>> VNPayWebhook([FromForm] Dictionary<string, string> data) =>
        OkResponse(await _paymentService.ProcessCallbackAsync("vnpay", data));

    [HttpPost("webhooks/momo")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<PaymentDto>>> MoMoWebhook([FromBody] Dictionary<string, string> data) =>
        OkResponse(await _paymentService.ProcessCallbackAsync("momo", data));

    [HttpPost("webhooks/zalopay")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<PaymentDto>>> ZaloPayWebhook([FromBody] Dictionary<string, string> data) =>
        OkResponse(await _paymentService.ProcessCallbackAsync("zalopay", data));
}
