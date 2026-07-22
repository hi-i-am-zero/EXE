using System.Net.Http.Json;
using System.Text.Json.Serialization;
using AutoWork.Infrastructure.Settings;
using Microsoft.Extensions.Logging;

namespace AutoWork.Infrastructure.Services;

internal sealed class BrevoEmailSender
{
    private readonly EmailSettings _settings;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger _logger;

    public BrevoEmailSender(
        EmailSettings settings,
        IHttpClientFactory httpClientFactory,
        ILogger logger)
    {
        _settings = settings;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken)
    {
        var fromEmail = EmailDeliveryResolver.GetFromEmail(_settings);
        var payload = new BrevoSendRequest
        {
            Sender = new BrevoPerson { Name = _settings.FromName, Email = fromEmail },
            To = [new BrevoPerson { Email = to }],
            Subject = subject,
            HtmlContent = htmlBody
        };

        using var client = _httpClientFactory.CreateClient(nameof(BrevoEmailSender));
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.brevo.com/v3/smtp/email");
        request.Headers.Add("api-key", _settings.BrevoApiKey.Trim());
        request.Content = JsonContent.Create(payload);

        using var response = await client.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if ((int)response.StatusCode == 401 && body.Contains("unrecognised IP", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "Brevo từ chối gửi mail vì IP máy bạn chưa được phép. " +
                    "Vào https://app.brevo.com/security/authorised_ips → bấm Deactivate for API keys.");
            }

            if ((int)response.StatusCode == 403 && body.Contains("not yet activated", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "Tài khoản Brevo chưa được kích hoạt gửi mail. Hoàn tất xác minh trên Brevo " +
                    "(email/số điện thoại) hoặc liên hệ contact@brevo.com. Tạm thời dùng Gmail SMTP: scripts/configure-email-gmail.ps1");
            }

            throw new InvalidOperationException(
                $"Brevo gửi email thất bại ({(int)response.StatusCode}): {body}");
        }

        _logger.LogInformation("Brevo email sent to {To}: {Subject}", to, subject);
    }

    private sealed class BrevoSendRequest
    {
        [JsonPropertyName("sender")]
        public BrevoPerson Sender { get; set; } = new();

        [JsonPropertyName("to")]
        public List<BrevoPerson> To { get; set; } = [];

        [JsonPropertyName("subject")]
        public string Subject { get; set; } = string.Empty;

        [JsonPropertyName("htmlContent")]
        public string HtmlContent { get; set; } = string.Empty;
    }

    private sealed class BrevoPerson
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;
    }
}
