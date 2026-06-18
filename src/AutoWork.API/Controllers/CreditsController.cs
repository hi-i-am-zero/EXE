using AutoWork.Application.DTOs.Credits;
using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Application.Interfaces.Services;
using AutoWork.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoWork.API.Controllers;

[Authorize]
[Route("api/credits")]
public class CreditsController : ApiControllerBase
{
    private readonly ICreditService _creditService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public CreditsController(ICreditService creditService, IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _creditService = creditService;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<CreditDto>>> GetBalance()
    {
        var balance = await _creditService.GetBalanceAsync(_currentUser.UserId!.Value);
        return OkResponse(balance ?? new CreditDto { UserId = _currentUser.UserId!.Value });
    }

    [HttpGet("transactions")]
    public async Task<ActionResult<ApiResponse<PaginatedResult<CreditTransactionDto>>>> GetTransactions(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = _currentUser.UserId!.Value;
        var transactions = await _unitOfWork.Credits.GetTransactionsByUserIdAsync(userId, page, pageSize);
        var total = await _unitOfWork.Credits.CountTransactionsByUserIdAsync(userId);

        var items = transactions.Select(t => new CreditTransactionDto
        {
            Id = t.Id,
            UserId = t.UserId,
            Amount = t.Amount,
            BalanceAfter = t.BalanceAfter,
            Type = t.Type,
            Description = t.Description ?? string.Empty,
            CreatedAt = t.CreatedAt
        }).ToList();

        return OkResponse(PaginatedResult<CreditTransactionDto>.Create(items, total, page, pageSize));
    }
}
