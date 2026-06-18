using AutoWork.Application.DTOs.Auth;
using MediatR;

namespace AutoWork.Application.Features.Auth.Commands;

public class LoginCommand : IRequest<LoginResponse>
{
    public LoginRequest Request { get; set; } = null!;
}
