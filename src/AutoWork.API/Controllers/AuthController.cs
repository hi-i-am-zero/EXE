using AutoWork.Application.DTOs.Auth;
using AutoWork.Application.Features.Auth.Commands;
using AutoWork.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoWork.API.Controllers;

[Route("api/auth")]
public class AuthController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator) => _mediator = mediator;

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Register([FromBody] RegisterRequest request) =>
        OkResponse(await _mediator.Send(new RegisterCommand { Request = request }), "Registration successful.");

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Login([FromBody] LoginRequest request) =>
        OkResponse(await _mediator.Send(new LoginCommand { Request = request }), "Login successful.");

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Refresh([FromBody] RefreshTokenRequest request) =>
        OkResponse(await _mediator.Send(new RefreshTokenCommand { Request = request }));

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse>> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        await _mediator.Send(new ForgotPasswordCommand { Email = request.Email });
        return OkResponse("If the email exists, a reset code has been sent.");
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse>> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        await _mediator.Send(new ResetPasswordCommand
        {
            Token = request.Token,
            OtpCode = request.OtpCode,
            NewPassword = request.NewPassword,
            ConfirmPassword = request.ConfirmPassword
        });
        return OkResponse("Password reset successful.");
    }
}
