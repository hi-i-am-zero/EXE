using System.Security.Claims;
using AutoWork.Domain.Entities;

namespace AutoWork.Application.Interfaces.Services;

public interface IJwtTokenService
{
    string GenerateAccessToken(User user, IEnumerable<string> roles);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    DateTime GetAccessTokenExpiration();
}
