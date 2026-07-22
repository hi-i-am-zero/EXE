using Microsoft.Extensions.Logging;

namespace AutoWork.Infrastructure.Services;

internal sealed class DevFileEmailSender
{
    private readonly ILogger _logger;
    private readonly string _outputDirectory;

    public DevFileEmailSender(ILogger logger, string? outputDirectory = null)
    {
        _logger = logger;
        _outputDirectory = outputDirectory ?? Path.Combine(AppContext.BaseDirectory, "logs", "emails");
    }

    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(_outputDirectory);

        var safeTo = string.Concat(to.Select(ch => char.IsLetterOrDigit(ch) ? ch : '_'));
        var fileName = $"{DateTime.UtcNow:yyyyMMdd_HHmmss}_{safeTo}.html";
        var filePath = Path.Combine(_outputDirectory, fileName);

        var content = $"""
            <!DOCTYPE html>
            <html lang="vi">
            <head><meta charset="utf-8"/><title>{subject}</title></head>
            <body style="font-family:Inter,Arial,sans-serif;padding:16px">
              <p><strong>To:</strong> {to}</p>
              <p><strong>Subject:</strong> {subject}</p>
              <hr/>
              {htmlBody}
            </body>
            </html>
            """;

        await File.WriteAllTextAsync(filePath, content, cancellationToken);
        _logger.LogWarning(
            "DEV email saved for {To} → {FilePath} (mở file này để xem link xác nhận)",
            to,
            filePath);
    }
}
