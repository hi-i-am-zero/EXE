using AutoWork.Application.DTOs.Auth;
using MediatR;

namespace AutoWork.Application.Features.Auth.Commands;

public class RegisterCommand : IRequest<RegisterResponse>
{
    public RegisterRequest Request { get; set; } = null!;
}
