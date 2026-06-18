using MediatR;

namespace AutoWork.Application.Features.Auth.Commands;

public class ForgotPasswordCommand : IRequest<bool>
{
    public string Email { get; set; } = string.Empty;
}
