using AutoWork.Application.Common.Exceptions;
using AutoWork.Application.DTOs.Auth;
using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Application.Interfaces.Services;
using AutoWork.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AutoWork.Application.Features.Auth.Commands;

public class ResendVerificationEmailCommand : IRequest<RegisterResponse>
{
    public string Email { get; set; } = string.Empty;
}

public class ResendVerificationEmailCommandValidator : AbstractValidator<ResendVerificationEmailCommand>
{
    public ResendVerificationEmailCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}

public class ResendVerificationEmailCommandHandler : IRequestHandler<ResendVerificationEmailCommand, RegisterResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<ResendVerificationEmailCommandHandler> _logger;

    public ResendVerificationEmailCommandHandler(
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        IHostEnvironment environment,
        ILogger<ResendVerificationEmailCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _environment = environment;
        _logger = logger;
    }

    public async Task<RegisterResponse> Handle(ResendVerificationEmailCommand command, CancellationToken cancellationToken)
    {
        var email = command.Email.Trim().ToLowerInvariant();

        var user = await _unitOfWork.Users.GetByEmailAsync(email, cancellationToken);
        if (user is not null && user.EmailVerified)
            return new RegisterResponse { Email = email, RequiresEmailVerification = true };

        var pending = await _unitOfWork.PendingRegistrations.GetActiveByEmailAsync(email, cancellationToken);
        if (pending is not null)
            return await ResendPendingAsync(pending, cancellationToken);

        if (user is not null && !user.EmailVerified)
            return await ResendLegacyUserAsync(user, cancellationToken);

        return new RegisterResponse { Email = email, RequiresEmailVerification = true };
    }

    private async Task<RegisterResponse> ResendPendingAsync(
        PendingRegistration pending,
        CancellationToken cancellationToken)
    {
        if (!_emailService.IsConfigured)
            throw new BadRequestException("Hệ thống chưa cấu hình email (Brevo hoặc SMTP).");

        pending.Token = Guid.NewGuid().ToString("N");
        pending.ExpiresAt = DateTime.UtcNow.AddHours(24);
        pending.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.PendingRegistrations.UpdateAsync(pending, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await SendVerificationAsync(pending.Email, pending.FirstName, pending.Token, cancellationToken);
    }

    private async Task<RegisterResponse> ResendLegacyUserAsync(User user, CancellationToken cancellationToken)
    {
        if (!_emailService.IsConfigured)
            throw new BadRequestException("Hệ thống chưa cấu hình email (Brevo hoặc SMTP).");

        await _unitOfWork.Users.InvalidateEmailVerificationTokensAsync(user.Id, cancellationToken);

        var verificationToken = Guid.NewGuid().ToString("N");
        await _unitOfWork.Users.AddEmailVerificationTokenAsync(new EmailVerificationToken
        {
            UserId = user.Id,
            Token = verificationToken,
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await SendVerificationAsync(user.Email, user.FirstName, verificationToken, cancellationToken);
    }

    private async Task<RegisterResponse> SendVerificationAsync(
        string email,
        string firstName,
        string token,
        CancellationToken cancellationToken)
    {
        var verifyUrl = _emailService.BuildEmailVerificationUrl(token);

        try
        {
            await _emailService.SendEmailVerificationAsync(email, firstName, verifyUrl, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to resend verification email to {Email}", email);

            if (_environment.IsDevelopment())
            {
                return new RegisterResponse
                {
                    Email = email,
                    RequiresEmailVerification = true,
                    EmailSent = false,
                    DeliveryMessage = ex.Message,
                    DevVerificationUrl = verifyUrl
                };
            }

            throw new BadRequestException("Không gửi được email xác nhận. Kiểm tra cấu hình Brevo hoặc SMTP.");
        }

        return new RegisterResponse
        {
            Email = email,
            RequiresEmailVerification = true,
            EmailSent = true
        };
    }
}
