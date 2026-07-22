using AutoWork.Application.Common.Exceptions;
using AutoWork.Application.Common.Helpers;
using AutoWork.Application.DTOs.Auth;
using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Application.Interfaces.Services;
using AutoWork.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AutoWork.Application.Features.Auth.Commands;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<RegisterCommandHandler> _logger;

    public RegisterCommandHandler(
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        IHostEnvironment environment,
        ILogger<RegisterCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _environment = environment;
        _logger = logger;
    }

    public async Task<RegisterResponse> Handle(RegisterCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var email = request.Email.Trim().ToLowerInvariant();

        var existingUser = await _unitOfWork.Users.GetByEmailAsync(email, cancellationToken);
        if (existingUser is not null)
        {
            if (existingUser.EmailVerified)
                throw new BadRequestException("Email đã được đăng ký.");

            await _unitOfWork.Users.DeleteUnverifiedUserAsync(existingUser.Id, cancellationToken);
        }

        var normalizedPhone = PhoneHelper.Normalize(request.Phone)
            ?? throw new BadRequestException("Số điện thoại là bắt buộc.");

        if (await _unitOfWork.Users.PhoneExistsAsync(normalizedPhone, cancellationToken: cancellationToken))
            throw new BadRequestException("Số điện thoại đã được đăng ký.");

        var pendingByPhone = await _unitOfWork.PendingRegistrations.GetActiveByPhoneAsync(normalizedPhone, cancellationToken);
        if (pendingByPhone is not null && !string.Equals(pendingByPhone.Email, email, StringComparison.OrdinalIgnoreCase))
            throw new BadRequestException("Số điện thoại đã được đăng ký.");

        Guid? referredByUserId = null;
        if (!string.IsNullOrWhiteSpace(request.ReferralCode))
        {
            var referrer = await _unitOfWork.Users.GetByReferralCodeAsync(request.ReferralCode, cancellationToken);
            referredByUserId = referrer?.Id;
        }

        await _unitOfWork.PendingRegistrations.RemoveActiveByEmailAsync(email, cancellationToken);

        var verificationToken = Guid.NewGuid().ToString("N");
        var pending = new PendingRegistration
        {
            Email = email,
            PasswordHash = PasswordHelper.Hash(request.Password),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Phone = normalizedPhone,
            ReferralCode = GenerateReferralCode(),
            ReferredByUserId = referredByUserId,
            Token = verificationToken,
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        };

        await _unitOfWork.PendingRegistrations.AddAsync(pending, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var verifyUrl = _emailService.BuildEmailVerificationUrl(verificationToken);

        try
        {
            await _emailService.SendEmailVerificationAsync(
                pending.Email,
                pending.FirstName,
                verifyUrl,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send verification email to {Email}", pending.Email);

            if (!_environment.IsDevelopment())
            {
                await _unitOfWork.PendingRegistrations.RemoveActiveByEmailAsync(email, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                throw new BadRequestException(
                    "Không gửi được email xác nhận. Kiểm tra cấu hình Brevo hoặc SMTP.");
            }

            return new RegisterResponse
            {
                Email = pending.Email,
                RequiresEmailVerification = true,
                EmailSent = false,
                DeliveryMessage = ex.Message,
                DevVerificationUrl = verifyUrl
            };
        }

        return new RegisterResponse
        {
            Email = pending.Email,
            RequiresEmailVerification = true,
            EmailSent = true
        };
    }

    private static string GenerateReferralCode()
    {
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray())
            .Replace("+", string.Empty)
            .Replace("/", string.Empty)
            .Replace("=", string.Empty)[..8]
            .ToUpperInvariant();
    }
}
