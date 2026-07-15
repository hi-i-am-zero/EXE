using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Application.Interfaces.Services;
using AutoWork.Domain.Entities;
using MediatR;

namespace AutoWork.Application.Features.Auth.Commands;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;

    public ForgotPasswordCommandHandler(IUnitOfWork unitOfWork, IEmailService emailService)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
    }

    public async Task<bool> Handle(ForgotPasswordCommand command, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(command.Email.Trim().ToLowerInvariant(), cancellationToken);

        if (user is null)
        {
            return true;
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

        await _emailService.SendPasswordResetEmailAsync(user.Email, otpCode, resetToken, cancellationToken);

        return true;
    }
}
