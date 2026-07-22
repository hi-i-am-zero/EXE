using AutoWork.Infrastructure.Settings;

namespace AutoWork.Infrastructure.Services;

internal enum EmailDeliveryMode
{
    None,
    Smtp,
    Brevo,
    DevFile
}

internal static class EmailDeliveryResolver
{
    public static EmailDeliveryMode Resolve(EmailSettings settings, bool isDevelopment)
    {
        var provider = (settings.Provider ?? "Auto").Trim();

        if (provider.Equals("DevFile", StringComparison.OrdinalIgnoreCase))
            return isDevelopment ? EmailDeliveryMode.DevFile : EmailDeliveryMode.None;

        if (provider.Equals("Brevo", StringComparison.OrdinalIgnoreCase) && HasBrevo(settings))
            return EmailDeliveryMode.Brevo;

        if (provider.Equals("Smtp", StringComparison.OrdinalIgnoreCase) && HasSmtp(settings))
            return EmailDeliveryMode.Smtp;

        if (!provider.Equals("Auto", StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(provider))
        {
            return EmailDeliveryMode.None;
        }

        if (HasBrevo(settings))
            return EmailDeliveryMode.Brevo;

        if (HasSmtp(settings))
            return EmailDeliveryMode.Smtp;

        if (isDevelopment && settings.UseDevFileFallback)
            return EmailDeliveryMode.DevFile;

        return EmailDeliveryMode.None;
    }

    public static bool IsConfigured(EmailSettings settings, bool isDevelopment) =>
        Resolve(settings, isDevelopment) != EmailDeliveryMode.None;

    private static bool HasBrevo(EmailSettings settings) =>
        !string.IsNullOrWhiteSpace(settings.BrevoApiKey)
        && !string.IsNullOrWhiteSpace(GetFromEmail(settings));

    private static bool HasSmtp(EmailSettings settings) =>
        !string.IsNullOrWhiteSpace(settings.SmtpHost)
        && !string.IsNullOrWhiteSpace(settings.Username)
        && !string.IsNullOrWhiteSpace(NormalizePassword(settings.Password));

    public static string GetFromEmail(EmailSettings settings) =>
        string.IsNullOrWhiteSpace(settings.FromEmail)
            ? settings.Username
            : settings.FromEmail;

    public static string NormalizePassword(string? password) =>
        (password ?? string.Empty).Replace(" ", string.Empty).Trim();
}
