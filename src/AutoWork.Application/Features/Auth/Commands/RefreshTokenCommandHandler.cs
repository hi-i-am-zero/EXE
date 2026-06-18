using AutoWork.Application.Common.Exceptions;
using AutoWork.Application.DTOs.Auth;
using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Application.Interfaces.Services;
using AutoWork.Domain.Entities;
using MediatR;

namespace AutoWork.Application.Features.Auth.Commands;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenService _jwtTokenService;

    public RefreshTokenCommandHandler(IUnitOfWork unitOfWork, IJwtTokenService jwtTokenService)
    {
        _unitOfWork = unitOfWork;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResponse> Handle(RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var principal = _jwtTokenService.GetPrincipalFromExpiredToken(request.AccessToken);

        if (principal is null)
        {
            throw new UnauthorizedException("Invalid access token.");
        }

        var userIdClaim = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedException("Invalid token claims.");
        }

        var storedRefreshToken = await _unitOfWork.Users.GetRefreshTokenAsync(request.RefreshToken, cancellationToken);
        if (storedRefreshToken is null || storedRefreshToken.UserId != userId || !storedRefreshToken.IsActive)
        {
            throw new UnauthorizedException("Invalid refresh token.");
        }

        var user = await _unitOfWork.Users.GetByIdWithRolesAsync(userId, cancellationToken);
        if (user is null || !user.IsActive)
        {
            throw new UnauthorizedException("User account is not active.");
        }

        storedRefreshToken.IsRevoked = true;
        storedRefreshToken.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Users.UpdateRefreshToken(storedRefreshToken);

        var roles = user.UserRoles.Select(ur => ur.Role.Name).DefaultIfEmpty("User").ToList();
        var newAccessToken = _jwtTokenService.GenerateAccessToken(user, roles);
        var newRefreshTokenValue = _jwtTokenService.GenerateRefreshToken();

        var newRefreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = newRefreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            ReplacedByToken = newRefreshTokenValue
        };

        storedRefreshToken.ReplacedByToken = newRefreshTokenValue;
        await _unitOfWork.Users.AddRefreshTokenAsync(newRefreshToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResponse
        {
            UserId = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            AccessToken = newAccessToken,
            RefreshToken = newRefreshTokenValue,
            ExpiresAt = _jwtTokenService.GetAccessTokenExpiration(),
            Roles = roles
        };
    }
}
