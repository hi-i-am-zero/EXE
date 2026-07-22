using AutoWork.Application.DTOs.Auth;
using MediatR;

namespace AutoWork.Application.Features.Auth.Commands;

public class ForgotPasswordCommand : IRequest<ForgotPasswordResponse>
{
    public string Email { get; set; } = string.Empty;
}
