namespace AutoWork.Application.DTOs.Auth;

public class RegisterResponse
{
    public string Email { get; set; } = string.Empty;
    public bool RequiresEmailVerification { get; set; } = true;
    public bool EmailSent { get; set; }
    public string? DeliveryMessage { get; set; }
    public string? DevVerificationUrl { get; set; }
}
