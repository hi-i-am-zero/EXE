using System.Net;
using System.Net.Mail;
using AutoWork.Application.Interfaces.Services;
using AutoWork.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AutoWork.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default) =>
        SendInternalAsync(to, subject, body, cancellationToken);

    public Task SendPasswordResetEmailAsync(string to, string otpCode, string resetToken, CancellationToken cancellationToken = default)
    {
        var body = $"""
            <h2>Đặt lại mật khẩu AutoWork</h2>
            <p>Mã OTP của bạn: <strong>{otpCode}</strong></p>
            <p>Mã này có hiệu lực trong 15 phút.</p>
            <p>Token: {resetToken}</p>
            """;
        return SendInternalAsync(to, "Đặt lại mật khẩu AutoWork", body, cancellationToken);
    }

    public Task SendWelcomeEmailAsync(string to, string firstName, CancellationToken cancellationToken = default)
    {
        var body = $"""
            <h2>Chào mừng {firstName}!</h2>
            <p>Tài khoản AutoWork của bạn đã được tạo thành công.</p>
            """;
        return SendInternalAsync(to, "Chào mừng đến với AutoWork", body, cancellationToken);
    }

    public Task SendWelcomeAsync(string email, string name, CancellationToken cancellationToken = default) =>
        SendWelcomeEmailAsync(email, name, cancellationToken);

    public Task SendOtpAsync(string email, string otp, CancellationToken cancellationToken = default)
    {
        var body = $"<p>Mã OTP của bạn: <strong>{otp}</strong></p>";
        return SendInternalAsync(email, "Mã OTP AutoWork", body, cancellationToken);
    }

    public Task SendNotificationAsync(string email, string subject, string body, CancellationToken cancellationToken = default) =>
        SendInternalAsync(email, subject, body, cancellationToken);

    private async Task SendInternalAsync(string to, string subject, string body, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_settings.SmtpHost))
        {
            _logger.LogWarning("Email not configured. Would send to {To}: {Subject}", to, subject);
            return;
        }

        using var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(_settings.Username, _settings.Password)
        };

        using var message = new MailMessage
        {
            From = new MailAddress(_settings.FromEmail, _settings.FromName),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };
        message.To.Add(to);

        await client.SendMailAsync(message, cancellationToken);
        _logger.LogInformation("Email sent to {To}", to);
    }
}
