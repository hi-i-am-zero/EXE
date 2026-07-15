using AutoWork.Application.Common.Exceptions;
using AutoWork.Application.DTOs.Credits;
using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Application.Interfaces.Services;
using AutoWork.Domain.Entities;
using AutoWork.Shared.Enums;
using Microsoft.Extensions.Logging;

namespace AutoWork.Infrastructure.Services;

public class CreditService : ICreditService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreditService> _logger;

    public CreditService(IUnitOfWork unitOfWork, ILogger<CreditService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<CreditDto?> GetBalanceAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var credit = await _unitOfWork.Credits.GetByUserIdAsync(userId, cancellationToken);
        if (credit is null)
        {
            return null;
        }

        return MapToDto(credit);
    }

    public async Task<bool> HasSufficientCreditsAsync(Guid userId, int amount, CancellationToken cancellationToken = default)
    {
        // DEMO PHASE: luôn cho phép, chưa chặn theo credit — billing/giới hạn số lần dùng sẽ làm ở
        // bản cập nhật sau khi các tính năng chính (Brand Memory, Chiến dịch tự động, đăng bài) đã
        // demo ổn. Việc trừ credit vẫn ghi nhận bình thường (xem DeductCreditsAsync) để không mất
        // dữ liệu lịch sử dùng — chỉ tắt phần CHẶN khi không đủ.
        return true;
    }

    public async Task<CreditTransactionDto> DeductCreditsAsync(
        Guid userId,
        int amount,
        CreditTransactionType type,
        string description,
        string? referenceType = null,
        Guid? referenceId = null,
        CancellationToken cancellationToken = default)
    {
        if (amount <= 0)
        {
            throw new BadRequestException("Deduction amount must be greater than zero.");
        }

        var credit = await GetOrCreateCreditAsync(userId, cancellationToken);

        // DEMO PHASE: không chặn khi thiếu credit (xem ghi chú ở HasSufficientCreditsAsync) —
        // vẫn trừ bình thường (có thể âm), để lịch sử CreditTransaction phản ánh đúng mức đã dùng,
        // phục vụ việc bật lại billing thật ở bản cập nhật sau mà không cần sửa logic tính toán.
        if (credit.Balance < amount)
        {
            _logger.LogWarning(
                "User {UserId} deducting {Amount} credits with insufficient balance {Balance} — allowed in demo phase.",
                userId, amount, credit.Balance);
        }

        credit.Balance -= amount;
        credit.TotalUsed += amount;
        credit.UpdatedAt = DateTime.UtcNow;

        var transaction = CreateTransaction(credit, userId, -amount, type, description, referenceType, referenceId);
        await _unitOfWork.Credits.AddTransactionAsync(transaction, cancellationToken);
        await _unitOfWork.Credits.UpdateAsync(credit, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deducted {Amount} credits from user {UserId}. Balance: {Balance}", amount, userId, credit.Balance);
        return MapTransactionToDto(transaction);
    }

    public async Task<CreditTransactionDto> AddCreditsAsync(
        Guid userId,
        int amount,
        CreditTransactionType type,
        string description,
        CancellationToken cancellationToken = default)
    {
        if (amount <= 0)
        {
            throw new BadRequestException("Credit amount must be greater than zero.");
        }

        var credit = await GetOrCreateCreditAsync(userId, cancellationToken);
        credit.Balance += amount;
        credit.TotalEarned += amount;
        credit.UpdatedAt = DateTime.UtcNow;

        var transaction = CreateTransaction(credit, userId, amount, type, description, null, null);
        await _unitOfWork.Credits.AddTransactionAsync(transaction, cancellationToken);
        await _unitOfWork.Credits.UpdateAsync(credit, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Added {Amount} credits to user {UserId}. Balance: {Balance}", amount, userId, credit.Balance);
        return MapTransactionToDto(transaction);
    }

    private async Task<Credit> GetOrCreateCreditAsync(Guid userId, CancellationToken cancellationToken)
    {
        var credit = await _unitOfWork.Credits.GetByUserIdAsync(userId, cancellationToken);
        if (credit is not null)
        {
            return credit;
        }

        credit = new Credit
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Balance = 0,
            TotalEarned = 0,
            TotalUsed = 0,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Credits.AddAsync(credit, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return credit;
    }

    private static CreditTransaction CreateTransaction(
        Credit credit,
        Guid userId,
        int amount,
        CreditTransactionType type,
        string description,
        string? referenceType,
        Guid? referenceId) =>
        new()
        {
            Id = Guid.NewGuid(),
            CreditId = credit.Id,
            UserId = userId,
            Type = (int)type,
            Amount = amount,
            BalanceAfter = credit.Balance,
            Description = description,
            ReferenceType = referenceType,
            ReferenceId = referenceId,
            CreatedAt = DateTime.UtcNow
        };

    private static CreditDto MapToDto(Credit credit) =>
        new()
        {
            UserId = credit.UserId,
            Balance = credit.Balance,
            TotalEarned = credit.TotalEarned,
            TotalUsed = credit.TotalUsed
        };

    private static CreditTransactionDto MapTransactionToDto(CreditTransaction transaction) =>
        new()
        {
            Id = transaction.Id,
            UserId = transaction.UserId,
            Amount = transaction.Amount,
            BalanceAfter = transaction.BalanceAfter,
            Type = transaction.Type,
            Description = transaction.Description ?? string.Empty,
            CreatedAt = transaction.CreatedAt
        };
}
