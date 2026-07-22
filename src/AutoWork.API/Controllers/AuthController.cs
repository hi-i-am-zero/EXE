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
    public async Task<ActionResult<ApiResponse<RegisterResponse>>> Register([FromBody] RegisterRequest request)
    {
        var result = await _mediator.Send(new RegisterCommand { Request = request });
        return OkResponse(result, "Đăng ký thành công. Vui lòng kiểm tra email để xác nhận tài khoản.");
    }

    [HttpGet("verify-email")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> VerifyEmail([FromQuery] string token)
    {
        var result = await _mediator.Send(new VerifyEmailCommand { Token = token });
        return OkResponse(result, "Xác nhận email thành công.");
    }

    [HttpPost("resend-verification")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<RegisterResponse>>> ResendVerification([FromBody] ResendVerificationRequest request)
    {
        var result = await _mediator.Send(new ResendVerificationEmailCommand { Email = request.Email });
        return OkResponse(result, "Nếu email chưa xác nhận, chúng tôi đã gửi lại link xác nhận.");
    }

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
    public async Task<ActionResult<ApiResponse<ForgotPasswordResponse>>> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var result = await _mediator.Send(new ForgotPasswordCommand { Email = request.Email });
        return OkResponse(result, "If the email exists, a reset code has been sent.");
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

    [HttpPost("change-password")]
    [Authorize]
    public async Task<ActionResult<ApiResponse>> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        await _mediator.Send(new ChangePasswordCommand
        {
            CurrentPassword = request.CurrentPassword,
            NewPassword = request.NewPassword,
            ConfirmPassword = request.ConfirmPassword
        });
        return OkResponse("Password changed successfully.");
    }
}
