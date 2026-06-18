using AutoWork.Application.Common.Exceptions;
using AutoWork.Application.Common.Helpers;
using AutoWork.Application.DTOs.Auth;
using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Application.Interfaces.Services;
using AutoWork.Domain.Entities;
using AutoWork.Shared.Enums;
using MediatR;

namespace AutoWork.Application.Features.Auth.Commands;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenService _jwtTokenService;

    public RegisterCommandHandler(IUnitOfWork unitOfWork, IJwtTokenService jwtTokenService)
    {
        _unitOfWork = unitOfWork;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResponse> Handle(RegisterCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;

        if (await _unitOfWork.Users.EmailExistsAsync(request.Email, cancellationToken))
        {
            throw new BadRequestException("Email is already registered.");
        }

        Guid? referredByUserId = null;
        if (!string.IsNullOrWhiteSpace(request.ReferralCode))
        {
            var referrer = await _unitOfWork.Users.GetByReferralCodeAsync(request.ReferralCode, cancellationToken);
            referredByUserId = referrer?.Id;
        }

        var user = new User
        {
            Email = request.Email.Trim().ToLowerInvariant(),
            PasswordHash = PasswordHelper.Hash(request.Password),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            ReferralCode = GenerateReferralCode(),
            ReferredByUserId = referredByUserId,
            IsActive = true
        };

        await _unitOfWork.Users.AddAsync(user, cancellationToken);

        var credit = new Credit
        {
            UserId = user.Id,
            Balance = 100,
            TotalEarned = 100,
            TotalUsed = 0
        };

        await _unitOfWork.Credits.AddAsync(credit, cancellationToken);

        var roles = new List<string> { UserRoleType.User.ToString() };
        var accessToken = _jwtTokenService.GenerateAccessToken(user, roles);
        var refreshTokenValue = _jwtTokenService.GenerateRefreshToken();

        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        await _unitOfWork.Users.AddRefreshTokenAsync(refreshToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResponse
        {
            UserId = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            AccessToken = accessToken,
            RefreshToken = refreshTokenValue,
            ExpiresAt = _jwtTokenService.GetAccessTokenExpiration(),
            Roles = roles
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
