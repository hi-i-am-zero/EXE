using AutoWork.Application.Interfaces.Services;
using AutoWork.Infrastructure.Settings;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AutoWork.Infrastructure.Services;

public class AzureBlobStorageService : IStorageService
{
    private readonly BlobContainerClient _containerClient;
    private readonly StorageSettings _settings;
    private readonly ILogger<AzureBlobStorageService> _logger;

    public AzureBlobStorageService(IOptions<StorageSettings> settings, ILogger<AzureBlobStorageService> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        if (string.IsNullOrWhiteSpace(_settings.Azure.ConnectionString))
        {
            throw new InvalidOperationException("Azure blob storage connection string is not configured.");
        }

        _containerClient = new BlobContainerClient(_settings.Azure.ConnectionString, _settings.Azure.ContainerName);
        _containerClient.CreateIfNotExists(PublicAccessType.Blob);
    }

    public async Task<string> UploadAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        string? folder = null,
        CancellationToken cancellationToken = default)
    {
        var blobName = BuildBlobName(fileName, folder);
        var blobClient = _containerClient.GetBlobClient(blobName);

        var headers = new BlobHttpHeaders { ContentType = contentType };
        await blobClient.UploadAsync(fileStream, new BlobUploadOptions { HttpHeaders = headers }, cancellationToken);

        _logger.LogInformation("Uploaded blob: {BlobName}", blobName);
        return blobName;
    }

    public async Task DeleteAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        await _containerClient.DeleteBlobIfExistsAsync(storagePath, cancellationToken: cancellationToken);
        _logger.LogInformation("Deleted blob: {BlobName}", storagePath);
    }

    public async Task<Stream> DownloadAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        var blobClient = _containerClient.GetBlobClient(storagePath);
        if (!await blobClient.ExistsAsync(cancellationToken))
        {
            throw new FileNotFoundException("Blob not found.", storagePath);
        }

        var response = await blobClient.DownloadStreamingAsync(cancellationToken: cancellationToken);
        return response.Value.Content;
    }

    public string GetPublicUrl(string storagePath)
    {
        if (!string.IsNullOrWhiteSpace(_settings.Azure.CdnBaseUrl))
        {
            return $"{_settings.Azure.CdnBaseUrl.TrimEnd('/')}/{storagePath}";
        }

        return _containerClient.GetBlobClient(storagePath).Uri.ToString();
    }

    private static string BuildBlobName(string fileName, string? folder)
    {
        var safeFileName = $"{Guid.NewGuid():N}_{Path.GetFileName(fileName)}";
        return string.IsNullOrWhiteSpace(folder)
            ? safeFileName
            : $"{folder.Trim('/').Replace('\\', '/')}/{safeFileName}";
    }
}
