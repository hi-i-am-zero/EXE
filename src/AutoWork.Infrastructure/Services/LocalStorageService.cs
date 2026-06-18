using AutoWork.Application.Interfaces.Services;
using AutoWork.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AutoWork.Infrastructure.Services;

public class LocalStorageService : IStorageService
{
    private readonly StorageSettings _settings;
    private readonly ILogger<LocalStorageService> _logger;

    public LocalStorageService(IOptions<StorageSettings> settings, ILogger<LocalStorageService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
        Directory.CreateDirectory(GetRootPath());
    }

    public async Task<string> UploadAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        string? folder = null,
        CancellationToken cancellationToken = default)
    {
        var safeFileName = $"{Guid.NewGuid():N}_{Path.GetFileName(fileName)}";
        var relativeFolder = SanitizeFolder(folder);
        var relativePath = string.IsNullOrEmpty(relativeFolder)
            ? safeFileName
            : Path.Combine(relativeFolder, safeFileName).Replace('\\', '/');

        var fullPath = Path.Combine(GetRootPath(), relativePath.Replace('/', Path.DirectorySeparatorChar));
        var directory = Path.GetDirectoryName(fullPath)!;
        Directory.CreateDirectory(directory);

        await using var output = File.Create(fullPath);
        await fileStream.CopyToAsync(output, cancellationToken);

        _logger.LogInformation("Uploaded file to local storage: {Path}", relativePath);
        return relativePath;
    }

    public Task DeleteAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        var fullPath = GetFullPath(storagePath);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            _logger.LogInformation("Deleted local file: {Path}", storagePath);
        }

        return Task.CompletedTask;
    }

    public Task<Stream> DownloadAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        var fullPath = GetFullPath(storagePath);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException("File not found in local storage.", storagePath);
        }

        Stream stream = File.OpenRead(fullPath);
        return Task.FromResult(stream);
    }

    public string GetPublicUrl(string storagePath)
    {
        var baseUrl = _settings.Local.PublicBaseUrl.TrimEnd('/');
        return $"{baseUrl}/{storagePath.Replace('\\', '/')}";
    }

    private string GetRootPath() =>
        Path.GetFullPath(_settings.Local.RootPath);

    private string GetFullPath(string storagePath)
    {
        var root = GetRootPath();
        var combined = Path.GetFullPath(Path.Combine(root, storagePath.Replace('/', Path.DirectorySeparatorChar)));
        if (!combined.StartsWith(root, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Invalid storage path.");
        }

        return combined;
    }

    private static string SanitizeFolder(string? folder)
    {
        if (string.IsNullOrWhiteSpace(folder))
        {
            return string.Empty;
        }

        var segments = folder.Split(['/', '\\'], StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim('.', ' '))
            .Where(s => !string.IsNullOrWhiteSpace(s) && s != "..");

        return string.Join('/', segments);
    }
}
