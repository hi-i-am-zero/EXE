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
        var loginUrl = $"{_settings.AppBaseUrl.TrimEnd('/')}/login.html";
        var body = $"""
            <div style="font-family:Inter,Arial,sans-serif;max-width:560px;margin:0 auto;padding:24px">
              <h2 style="color:#5b4df2;margin:0 0 12px">FlowMate — Đặt lại mật khẩu</h2>
              <p>Bạn vừa yêu cầu đặt lại mật khẩu. Mã OTP của bạn:</p>
              <p style="font-size:28px;font-weight:800;letter-spacing:4px;color:#2d3450">{otpCode}</p>
              <p>Mã có hiệu lực trong <strong>15 phút</strong>.</p>
              <p>Quay lại trang đăng nhập, chọn <strong>Quên mật khẩu</strong> và nhập mã OTP cùng mật khẩu mới.</p>
              <p><a href="{loginUrl}" style="color:#5b4df2">Mở trang đăng nhập FlowMate</a></p>
              <p style="color:#9299af;font-size:13px">Nếu bạn không yêu cầu, hãy bỏ qua email này.</p>
            </div>
            """;
        return SendInternalAsync(to, "FlowMate — Mã OTP đặt lại mật khẩu", body, cancellationToken);
    }

    public Task SendWelcomeEmailAsync(string to, string firstName, CancellationToken cancellationToken = default)
    {
        var loginUrl = $"{_settings.AppBaseUrl.TrimEnd('/')}/login.html";
        var body = $"""
            <div style="font-family:Inter,Arial,sans-serif;max-width:560px;margin:0 auto;padding:24px">
              <h2 style="color:#5b4df2;margin:0 0 12px">Chào mừng đến FlowMate!</h2>
              <p>Xin chào <strong>{firstName}</strong>,</p>
              <p>Tài khoản của bạn đã được tạo thành công. Bạn có thể đăng nhập và bắt đầu tạo chiến dịch marketing ngay.</p>
              <p><a href="{loginUrl}" style="display:inline-block;background:#5b4df2;color:#fff;text-decoration:none;padding:10px 18px;border-radius:8px;font-weight:600">Đăng nhập FlowMate</a></p>
              <p style="color:#9299af;font-size:13px">Email xác nhận tự động từ FlowMate.</p>
            </div>
            """;
        return SendInternalAsync(to, "FlowMate — Xác nhận đăng ký tài khoản", body, cancellationToken);
    }

    public Task SendWelcomeAsync(string email, string name, CancellationToken cancellationToken = default) =>
        SendWelcomeEmailAsync(email, name, cancellationToken);

    public Task SendOtpAsync(string email, string otp, CancellationToken cancellationToken = default)
    {
        var body = $"""
            <div style="font-family:Inter,Arial,sans-serif;max-width:560px;margin:0 auto;padding:24px">
              <p>Mã OTP FlowMate của bạn:</p>
              <p style="font-size:28px;font-weight:800;letter-spacing:4px">{otp}</p>
            </div>
            """;
        return SendInternalAsync(email, "FlowMate — Mã OTP", body, cancellationToken);
    }

    public Task SendNotificationAsync(string email, string subject, string body, CancellationToken cancellationToken = default) =>
        SendInternalAsync(email, subject, body, cancellationToken);

    private async Task SendInternalAsync(string to, string subject, string body, CancellationToken cancellationToken)
    {
        if (!IsConfigured())
        {
            _logger.LogWarning(
                "Email chưa cấu hình (thiếu SmtpHost/Username/Password). Bỏ qua gửi tới {To}: {Subject}. " +
                "Xem appsettings.Development.local.json hoặc chạy scripts/configure-email.ps1",
                to,
                subject);
            return;
        }

        var fromEmail = string.IsNullOrWhiteSpace(_settings.FromEmail)
            ? _settings.Username
            : _settings.FromEmail;

        using var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
        {
            EnableSsl = _settings.UseSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            Credentials = new NetworkCredential(_settings.Username, _settings.Password)
        };

        using var message = new MailMessage
        {
            From = new MailAddress(fromEmail, _settings.FromName),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };
        message.To.Add(to);

        await client.SendMailAsync(message, cancellationToken);
        _logger.LogInformation("Email sent to {To}: {Subject}", to, subject);
    }

    private bool IsConfigured() =>
        !string.IsNullOrWhiteSpace(_settings.SmtpHost)
        && !string.IsNullOrWhiteSpace(_settings.Username)
        && !string.IsNullOrWhiteSpace(_settings.Password);
}
