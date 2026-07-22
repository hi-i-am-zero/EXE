namespace AutoWork.Infrastructure.Settings;

public class EmailSettings
{
    public const string SectionName = "EmailSettings";

    /// <summary>Smtp | Brevo | Auto (ưu tiên Brevo → SMTP → DevFile khi Development)</summary>
    public string Provider { get; set; } = "Auto";

    public string SmtpHost { get; set; } = string.Empty;

    public int SmtpPort { get; set; } = 587;

    public bool UseSsl { get; set; } = true;

    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string FromEmail { get; set; } = string.Empty;

    public string FromName { get; set; } = "FlowMate";

    public string AppBaseUrl { get; set; } = "https://localhost:7264";

    /// <summary>API key Brevo (Sendinblue) — gửi được tới mọi email người dùng.</summary>
    public string BrevoApiKey { get; set; } = string.Empty;

    /// <summary>Development: lưu email ra logs/emails khi chưa có SMTP/Brevo.</summary>
    public bool UseDevFileFallback { get; set; } = true;
}
