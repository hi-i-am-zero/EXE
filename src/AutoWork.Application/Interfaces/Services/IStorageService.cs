namespace AutoWork.Application.Interfaces.Services;

public interface IStorageService
{
    Task<string> UploadAsync(Stream fileStream, string fileName, string contentType, string? folder = null, CancellationToken cancellationToken = default);
    Task DeleteAsync(string storagePath, CancellationToken cancellationToken = default);
    Task<Stream> DownloadAsync(string storagePath, CancellationToken cancellationToken = default);
    string GetPublicUrl(string storagePath);
}
