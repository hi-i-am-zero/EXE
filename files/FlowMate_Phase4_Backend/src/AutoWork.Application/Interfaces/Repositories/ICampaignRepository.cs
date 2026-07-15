using AutoWork.Domain.Entities;

namespace AutoWork.Application.Interfaces.Repositories;

public interface ICampaignRepository : IRepository<Campaign>
{
    Task<IReadOnlyList<Campaign>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);

    Task<Campaign?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
}
