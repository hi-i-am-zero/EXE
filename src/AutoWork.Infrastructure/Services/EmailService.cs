using AutoWork.Application.Interfaces.Services;
using AutoWork.Infrastructure.Settings;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace AutoWork.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly IHostEnvironment _environment;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IOptions<EmailSettings> settings,
        IHostEnvironment environment,
        IHttpClientFactory httpClientFactory,
        ILogger<EmailService> logger)
    {
        _settings = settings.Value;
        _environment = environment;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public bool IsConfigured =>
        EmailDeliveryResolver.IsConfigured(_settings, _environment.IsDevelopment());

    public string BuildEmailVerificationUrl(string token) =>
        $"{_settings.AppBaseUrl.TrimEnd('/')}/verify-email.html?token={Uri.EscapeDataString(token)}";

    public Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default) =>
        SendInternalAsync(to, subject, body, cancellationToken);

    public Task SendEmailVerificationAsync(
        string to,
        string firstName,
        string verifyUrl,
        CancellationToken cancellationToken = default)
    {
        var body = $"""
            <div style="font-family:Inter,Arial,sans-serif;max-width:560px;margin:0 auto;padding:24px">
              <h2 style="color:#5b4df2;margin:0 0 12px">Xác nhận tài khoản FlowMate</h2>
              <p>Xin chào <strong>{firstName}</strong>,</p>
              <p>Cảm ơn bạn đã đăng ký. Nhấn nút bên dưới để xác nhận email và vào ứng dụng:</p>
              <p style="margin:24px 0">
                <a href="{verifyUrl}" style="display:inline-block;background:#5b4df2;color:#fff;text-decoration:none;padding:12px 22px;border-radius:10px;font-weight:700">
                  Xác nhận email
                </a>
              </p>
              <p style="color:#646b80;font-size:14px">Link có hiệu lực trong <strong>24 giờ</strong>.</p>
              <p style="color:#9299af;font-size:13px;word-break:break-all">Nếu nút không hoạt động, copy link: {verifyUrl}</p>
            </div>
            """;
        return SendInternalAsync(to, "FlowMate — Xác nhận đăng ký tài khoản", body, cancellationToken);
    }

    public Task SendPasswordResetEmailAsync(string to, string otpCode, string resetToken, CancellationToken cancellationToken = default)
    {
        var loginUrl = $"{_settings.AppBaseUrl.TrimEnd('/')}/login.html";
        var body = $"""
            <div style="font-family:Inter,Arial,sans-serif;max-width:560px;margin:0 auto;padding:24px">
              <h2 style="color:#5b4df2;margin:0 0 12px">FlowMate — Đặt lại mật khẩu</h2>
              <p>Mã OTP của bạn:</p>
              <p style="font-size:28px;font-weight:800;letter-spacing:4px;color:#2d3450">{otpCode}</p>
              <p>Mã có hiệu lực trong <strong>15 phút</strong>.</p>
              <p><a href="{loginUrl}" style="color:#5b4df2">Mở trang đăng nhập FlowMate</a></p>
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
              <p>Xin chào <strong>{firstName}</strong>, tài khoản của bạn đã sẵn sàng.</p>
              <p><a href="{loginUrl}" style="color:#5b4df2">Đăng nhập FlowMate</a></p>
            </div>
            """;
        return SendInternalAsync(to, "FlowMate — Chào mừng", body, cancellationToken);
    }

    public Task SendWelcomeAsync(string email, string name, CancellationToken cancellationToken = default) =>
        SendWelcomeEmailAsync(email, name, cancellationToken);

    public Task SendOtpAsync(string email, string otp, CancellationToken cancellationToken = default)
    {
        var body = $"<p>Mã OTP FlowMate: <strong>{otp}</strong></p>";
        return SendInternalAsync(email, "FlowMate — Mã OTP", body, cancellationToken);
    }

    public Task SendNotificationAsync(string email, string subject, string body, CancellationToken cancellationToken = default) =>
        SendInternalAsync(email, subject, body, cancellationToken);

    private async Task SendInternalAsync(string to, string subject, string body, CancellationToken cancellationToken)
    {
        var mode = EmailDeliveryResolver.Resolve(_settings, _environment.IsDevelopment());
        if (mode == EmailDeliveryMode.None)
        {
            throw new InvalidOperationException(
                "Email chưa cấu hình. Dùng Brevo API key, SMTP (Gmail/Outlook), hoặc bật DevFile trong Development.");
        }

        switch (mode)
        {
            case EmailDeliveryMode.Brevo:
                await new BrevoEmailSender(_settings, _httpClientFactory, _logger)
                    .SendAsync(to, subject, body, cancellationToken);
                return;

            case EmailDeliveryMode.DevFile:
                await new DevFileEmailSender(_logger)
                    .SendAsync(to, subject, body, cancellationToken);
                return;

            default:
                await SendViaSmtpAsync(to, subject, body, cancellationToken);
                return;
        }
    }

    private async Task SendViaSmtpAsync(string to, string subject, string body, CancellationToken cancellationToken)
    {
        var fromEmail = EmailDeliveryResolver.GetFromEmail(_settings);

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.FromName, fromEmail));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = body };

        using var client = new SmtpClient();
        var secureOption = _settings.SmtpPort == 465
            ? SecureSocketOptions.SslOnConnect
            : SecureSocketOptions.StartTls;

        await client.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort, secureOption, cancellationToken);
        await client.AuthenticateAsync(
            _settings.Username,
            EmailDeliveryResolver.NormalizePassword(_settings.Password),
            cancellationToken);
        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);

        _logger.LogInformation("SMTP email sent to {To}: {Subject}", to, subject);
    }
}
