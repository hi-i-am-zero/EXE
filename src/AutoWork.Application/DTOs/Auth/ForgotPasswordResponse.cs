namespace AutoWork.Application.DTOs.Auth;

public class ForgotPasswordResponse
{
    public string? ResetToken { get; set; }
    public string? DevOtpCode { get; set; }
}
