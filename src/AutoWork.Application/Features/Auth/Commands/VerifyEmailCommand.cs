using AutoWork.Application.Common.Exceptions;
using AutoWork.Application.DTOs.Auth;
using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Application.Interfaces.Services;
using AutoWork.Domain.Entities;
using AutoWork.Shared.Enums;
using MediatR;

namespace AutoWork.Application.Features.Auth.Commands;

public class VerifyEmailCommand : IRequest<AuthResponse>
{
    public string Token { get; set; } = string.Empty;
}

public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, AuthResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenService _jwtTokenService;

    public VerifyEmailCommandHandler(IUnitOfWork unitOfWork, IJwtTokenService jwtTokenService)
    {
        _unitOfWork = unitOfWork;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResponse> Handle(VerifyEmailCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Token))
            throw new BadRequestException("Token xác nhận không hợp lệ.");

        var token = command.Token.Trim();
        var pending = await _unitOfWork.PendingRegistrations.GetByTokenAsync(token, cancellationToken);
        if (pending is not null)
            return await ActivateFromPendingAsync(pending, cancellationToken);

        return await ActivateLegacyUserAsync(token, cancellationToken);
    }

    private async Task<AuthResponse> ActivateFromPendingAsync(
        PendingRegistration pending,
        CancellationToken cancellationToken)
    {
        if (pending.UsedAt != null || pending.ExpiresAt <= DateTime.UtcNow)
            throw new BadRequestException("Link xác nhận đã hết hạn. Vui lòng đăng ký lại hoặc gửi lại email.");

        if (await _unitOfWork.Users.EmailExistsAsync(pending.Email, cancellationToken))
            throw new BadRequestException("Email đã được kích hoạt. Hãy đăng nhập.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = pending.Email,
            PasswordHash = pending.PasswordHash,
            FirstName = pending.FirstName,
            LastName = pending.LastName,
            Phone = pending.Phone,
            ReferralCode = pending.ReferralCode,
            ReferredByUserId = pending.ReferredByUserId,
            IsActive = true,
            EmailVerified = true,
            LastLoginAt = DateTime.UtcNow
        };

        await _unitOfWork.Users.AddAsync(user, cancellationToken);
        await _unitOfWork.Credits.AddAsync(new Credit
        {
            UserId = user.Id,
            Balance = 100,
            TotalEarned = 100,
            TotalUsed = 0
        }, cancellationToken);

        pending.UsedAt = DateTime.UtcNow;
        pending.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.PendingRegistrations.UpdateAsync(pending, cancellationToken);

        return await IssueAuthResponseAsync(user, cancellationToken);
    }

    private async Task<AuthResponse> ActivateLegacyUserAsync(string token, CancellationToken cancellationToken)
    {
        var stored = await _unitOfWork.Users.GetEmailVerificationTokenAsync(token, cancellationToken)
            ?? throw new BadRequestException("Link xác nhận không hợp lệ hoặc đã hết hạn.");

        if (stored.UsedAt != null || stored.ExpiresAt <= DateTime.UtcNow)
            throw new BadRequestException("Link xác nhận đã hết hạn. Vui lòng đăng ký lại hoặc gửi lại email.");

        var user = stored.User;
        if (!user.IsActive)
            throw new BadRequestException("Tài khoản đã bị vô hiệu hóa.");

        user.EmailVerified = true;
        stored.UsedAt = DateTime.UtcNow;
        user.LastLoginAt = DateTime.UtcNow;

        await _unitOfWork.Users.UpdateAsync(user, cancellationToken);
        _unitOfWork.Users.UpdateEmailVerificationToken(stored);

        return await IssueAuthResponseAsync(user, cancellationToken);
    }

    private async Task<AuthResponse> IssueAuthResponseAsync(User user, CancellationToken cancellationToken)
    {
        var userWithRoles = await _unitOfWork.Users.GetByIdWithRolesAsync(user.Id, cancellationToken) ?? user;
        var roles = userWithRoles.UserRoles.Select(ur => ur.Role.Name).DefaultIfEmpty(UserRoleType.User.ToString()).ToList();

        var accessToken = _jwtTokenService.GenerateAccessToken(userWithRoles, roles);
        var refreshTokenValue = _jwtTokenService.GenerateRefreshToken();

        await _unitOfWork.Users.AddRefreshTokenAsync(new RefreshToken
        {
            UserId = user.Id,
            Token = refreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResponse
        {
            UserId = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Phone = user.Phone,
            AvatarUrl = user.AvatarUrl,
            AccessToken = accessToken,
            RefreshToken = refreshTokenValue,
            ExpiresAt = _jwtTokenService.GetAccessTokenExpiration(),
            Roles = roles
        };
    }
}
