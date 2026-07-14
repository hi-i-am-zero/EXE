using AutoWork.Application.DTOs.Auth;
using MediatR;

namespace AutoWork.Application.Features.Auth.Commands;

public class RefreshTokenCommand : IRequest<AuthResponse>
{
    public RefreshTokenRequest Request { get; set; } = null!;
}
