namespace AutoWork.Application.DTOs.Auth;

public class ForgotPasswordResponse
{
    public string? ResetToken { get; set; }
    public bool EmailSent { get; set; }
    public string? DeliveryMessage { get; set; }
    public string? DevOtpCode { get; set; }
}
