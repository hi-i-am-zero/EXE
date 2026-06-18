using AutoWork.Application.Common.Exceptions;
using AutoWork.Application.Common.Helpers;
using AutoWork.Application.DTOs.Auth;
using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Application.Interfaces.Services;
using AutoWork.Domain.Entities;
using MediatR;

namespace AutoWork.Application.Features.Auth.Commands;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenService _jwtTokenService;

    public LoginCommandHandler(IUnitOfWork unitOfWork, IJwtTokenService jwtTokenService)
    {
        _unitOfWork = unitOfWork;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<LoginResponse> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var user = await _unitOfWork.Users.GetByEmailAsync(request.Email.Trim().ToLowerInvariant(), cancellationToken);

        if (user is null || !user.IsActive || !PasswordHelper.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedException("Invalid email or password.");
        }

        user.LastLoginAt = DateTime.UtcNow;
        await _unitOfWork.Users.UpdateAsync(user, cancellationToken);

        var userWithRoles = await _unitOfWork.Users.GetByIdWithRolesAsync(user.Id, cancellationToken)
            ?? user;

        var roles = userWithRoles.UserRoles
            .Select(ur => ur.Role.Name)
            .DefaultIfEmpty("User")
            .ToList();

        var accessToken = _jwtTokenService.GenerateAccessToken(userWithRoles, roles);
        var refreshTokenValue = _jwtTokenService.GenerateRefreshToken();

        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        await _unitOfWork.Users.AddRefreshTokenAsync(refreshToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new LoginResponse
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
}
