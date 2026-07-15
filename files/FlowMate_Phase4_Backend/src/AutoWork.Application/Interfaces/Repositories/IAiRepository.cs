using AutoWork.Domain.Entities;

namespace AutoWork.Application.Interfaces.Repositories;

public interface IAiRepository : IRepository<AiGeneratedContent>
{
    Task<IReadOnlyList<AiGeneratedContent>> GetHistoryByUserIdAsync(Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountHistoryByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<int> CountGenerationsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AiPrompt>> GetPromptsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<AiPrompt?> GetPromptByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AiPrompt> AddPromptAsync(AiPrompt prompt, CancellationToken cancellationToken = default);
    Task DeletePromptAsync(AiPrompt prompt, CancellationToken cancellationToken = default);
}
