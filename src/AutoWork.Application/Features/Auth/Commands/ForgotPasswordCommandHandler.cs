using AutoWork.Application.DTOs.Auth;
using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Application.Interfaces.Services;
using AutoWork.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AutoWork.Application.Features.Auth.Commands;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, ForgotPasswordResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<ForgotPasswordCommandHandler> _logger;

    public ForgotPasswordCommandHandler(
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        IHostEnvironment environment,
        ILogger<ForgotPasswordCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _environment = environment;
        _logger = logger;
    }

    public async Task<ForgotPasswordResponse> Handle(ForgotPasswordCommand command, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(command.Email.Trim().ToLowerInvariant(), cancellationToken);

        if (user is null)
        {
            return new ForgotPasswordResponse();
        }

        var otpCode = Random.Shared.Next(100000, 999999).ToString();
        var resetToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

        var passwordResetToken = new PasswordResetToken
        {
            UserId = user.Id,
            Token = resetToken,
            OtpCode = otpCode,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15)
        };

        await _unitOfWork.Users.AddPasswordResetTokenAsync(passwordResetToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            await _emailService.SendPasswordResetEmailAsync(user.Email, otpCode, resetToken, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not send password reset email to {Email}", user.Email);
        }

        return new ForgotPasswordResponse
        {
            ResetToken = resetToken,
            DevOtpCode = _environment.IsDevelopment() ? otpCode : null
        };
    }
}
