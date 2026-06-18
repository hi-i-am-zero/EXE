namespace AutoWork.Infrastructure.Settings;

public class StorageSettings
{
    public const string SectionName = "StorageSettings";

    /// <summary>Local or Azure</summary>
    public string Provider { get; set; } = "Local";

    public LocalStorageSettings Local { get; set; } = new();

    public AzureBlobStorageSettings Azure { get; set; } = new();
}

public class LocalStorageSettings
{
    public string RootPath { get; set; } = "wwwroot/uploads";

    public string PublicBaseUrl { get; set; } = "/uploads";
}

public class AzureBlobStorageSettings
{
    public string ConnectionString { get; set; } = string.Empty;

    public string ContainerName { get; set; } = "media";

    public string? CdnBaseUrl { get; set; }
}
