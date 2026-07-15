using AutoWork.Application.Common.Exceptions;
using AutoWork.Application.Common.Helpers;
using AutoWork.Application.Interfaces.Repositories;
using FluentValidation;
using MediatR;

namespace AutoWork.Application.Features.Auth.Commands;

public class ResetPasswordCommand : IRequest<bool>
{
    public string Token { get; set; } = string.Empty;
    public string OtpCode { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Token).NotEmpty();
        RuleFor(x => x.OtpCode).NotEmpty().Length(6);
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(8);
        RuleFor(x => x.ConfirmPassword).Equal(x => x.NewPassword);
    }
}

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public ResetPasswordCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<bool> Handle(ResetPasswordCommand command, CancellationToken cancellationToken)
    {
        var resetToken = await _unitOfWork.Users.GetPasswordResetTokenAsync(command.Token, cancellationToken)
            ?? throw new BadRequestException("Invalid reset token.");

        if (resetToken.OtpCode != command.OtpCode || resetToken.UsedAt != null || resetToken.ExpiresAt <= DateTime.UtcNow)
            throw new BadRequestException("Invalid or expired OTP code.");

        var user = await _unitOfWork.Users.GetByIdAsync(resetToken.UserId, cancellationToken)
            ?? throw new BadRequestException("User not found.");

        user.PasswordHash = PasswordHelper.Hash(command.NewPassword);
        resetToken.UsedAt = DateTime.UtcNow;

        await _unitOfWork.Users.UpdateAsync(user, cancellationToken);
        _unitOfWork.Users.UpdatePasswordResetToken(resetToken);
        await _unitOfWork.Users.RevokeUserRefreshTokensAsync(user.Id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
