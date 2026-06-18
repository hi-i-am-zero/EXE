using AutoWork.Application.DTOs.AI;

namespace AutoWork.Application.Interfaces.Services;

public interface IAiContentService
{
    Task<GenerateContentResponse> GenerateContentAsync(Guid userId, GenerateContentRequest request, CancellationToken cancellationToken = default);
}
