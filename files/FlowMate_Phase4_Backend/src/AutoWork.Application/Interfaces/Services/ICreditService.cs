using AutoWork.Application.DTOs.Credits;
using AutoWork.Shared.Enums;

namespace AutoWork.Application.Interfaces.Services;

public interface ICreditService
{
    Task<CreditDto?> GetBalanceAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> HasSufficientCreditsAsync(Guid userId, int amount, CancellationToken cancellationToken = default);
    Task<CreditTransactionDto> DeductCreditsAsync(Guid userId, int amount, CreditTransactionType type, string description, string? referenceType = null, Guid? referenceId = null, CancellationToken cancellationToken = default);
    Task<CreditTransactionDto> AddCreditsAsync(Guid userId, int amount, CreditTransactionType type, string description, CancellationToken cancellationToken = default);
}
