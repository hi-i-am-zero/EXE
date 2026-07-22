namespace AutoWork.API.Configuration;

public sealed class EmailPropertiesConfigurationProvider : ConfigurationProvider
{
    private readonly string _path;

    public EmailPropertiesConfigurationProvider(string path) => _path = path;

    public override void Load()
    {
        if (!File.Exists(_path))
            return;

        var data = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        foreach (var rawLine in File.ReadAllLines(_path))
        {
            var line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith('#'))
                continue;

            var separator = line.IndexOf('=');
            if (separator <= 0)
                continue;

            var key = line[..separator].Trim();
            var value = line[(separator + 1)..].Trim();
            if (string.IsNullOrWhiteSpace(value))
                continue;

            switch (key)
            {
                case "mail.smtp.host":
                    data["EmailSettings:SmtpHost"] = value;
                    break;
                case "mail.smtp.port":
                    data["EmailSettings:SmtpPort"] = value;
                    break;
                case "mail.username":
                    data["EmailSettings:Username"] = value;
                    break;
                case "mail.password":
                    data["EmailSettings:Password"] = value;
                    break;
                case "mail.from":
                    data["EmailSettings:FromEmail"] = value;
                    break;
                case "mail.from.name":
                    data["EmailSettings:FromName"] = value;
                    break;
                case "app.base.url":
                    data["EmailSettings:AppBaseUrl"] = value;
                    break;
            }
        }

        if (!string.IsNullOrWhiteSpace(data.GetValueOrDefault("EmailSettings:Username")))
            data["EmailSettings:Provider"] = "Smtp";

        Data = data;
    }
}

public sealed class EmailPropertiesConfigurationSource : IConfigurationSource
{
    private readonly string _path;

    public EmailPropertiesConfigurationSource(string path) => _path = path;

    public IConfigurationProvider Build(IConfigurationBuilder builder) =>
        new EmailPropertiesConfigurationProvider(_path);
}

public static class EmailPropertiesConfigurationExtensions
{
    public static IConfigurationBuilder AddEmailPropertiesFile(
        this IConfigurationBuilder builder,
        string path = "email.properties")
    {
        builder.Add(new EmailPropertiesConfigurationSource(path));
        return builder;
    }
}
