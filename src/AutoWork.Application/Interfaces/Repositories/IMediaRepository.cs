using AutoWork.Domain.Entities;

namespace AutoWork.Application.Interfaces.Repositories;

public interface IMediaRepository : IRepository<MediaFile>
{
    Task<IReadOnlyList<MediaFile>> GetByUserIdAsync(Guid userId, string? folder = null, CancellationToken cancellationToken = default);
    Task<MediaFile?> GetByStoragePathAsync(string storagePath, CancellationToken cancellationToken = default);
}
