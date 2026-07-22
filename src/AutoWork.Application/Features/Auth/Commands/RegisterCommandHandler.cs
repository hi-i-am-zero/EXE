using AutoWork.Application.Common.Exceptions;
using AutoWork.Application.Common.Helpers;
using AutoWork.Application.DTOs.Auth;
using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Application.Interfaces.Services;
using AutoWork.Domain.Entities;
using AutoWork.Shared.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AutoWork.Application.Features.Auth.Commands;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IEmailService _emailService;
    private readonly ILogger<RegisterCommandHandler> _logger;

    public RegisterCommandHandler(
        IUnitOfWork unitOfWork,
        IJwtTokenService jwtTokenService,
        IEmailService emailService,
        ILogger<RegisterCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _jwtTokenService = jwtTokenService;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<AuthResponse> Handle(RegisterCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;

        if (await _unitOfWork.Users.EmailExistsAsync(request.Email, cancellationToken))
        {
            throw new BadRequestException("Email is already registered.");
        }

        var normalizedPhone = PhoneHelper.Normalize(request.Phone)
            ?? throw new BadRequestException("Phone number is required.");

        if (await _unitOfWork.Users.PhoneExistsAsync(normalizedPhone, cancellationToken: cancellationToken))
        {
            throw new BadRequestException("Phone number is already registered.");
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
            Phone = normalizedPhone,
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

        try
        {
            await _emailService.SendWelcomeEmailAsync(user.Email, user.FirstName, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not send registration email to {Email}", user.Email);
        }

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

    private static string GenerateReferralCode()
    {
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray())
            .Replace("+", string.Empty)
            .Replace("/", string.Empty)
            .Replace("=", string.Empty)[..8]
            .ToUpperInvariant();
    }
}
