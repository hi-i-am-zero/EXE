namespace AutoWork.Application.Interfaces.Services;

public interface IEmailService
{
    bool IsConfigured { get; }

    string BuildEmailVerificationUrl(string token);

    Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default);

    Task SendEmailVerificationAsync(string to, string firstName, string verifyUrl, CancellationToken cancellationToken = default);

    Task SendPasswordResetEmailAsync(string to, string otpCode, string resetToken, CancellationToken cancellationToken = default);

    Task SendWelcomeEmailAsync(string to, string firstName, CancellationToken cancellationToken = default);

    Task SendWelcomeAsync(string email, string name, CancellationToken cancellationToken = default);

    Task SendOtpAsync(string email, string otp, CancellationToken cancellationToken = default);

    Task SendNotificationAsync(string email, string subject, string body, CancellationToken cancellationToken = default);
}
