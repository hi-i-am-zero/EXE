using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Domain.Entities;
using AutoWork.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace AutoWork.Persistence.Repositories;

public class AiRepository : Repository<AiGeneratedContent>, IAiRepository
{
    public AiRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<AiGeneratedContent>> GetHistoryByUserIdAsync(
        Guid userId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default) =>
        await DbSet
            .AsNoTracking()
            .Include(a => a.AiPrompt)
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public async Task<int> CountHistoryByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
        await DbSet.CountAsync(a => a.UserId == userId, cancellationToken);

    public async Task<int> CountGenerationsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
        await DbSet.CountAsync(a => a.UserId == userId, cancellationToken);

    public async Task<IReadOnlyList<AiPrompt>> GetPromptsByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        await Context.AiPrompts
            .AsNoTracking()
            .Where(p => p.UserId == userId || p.IsSystem)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);

    public async Task<AiPrompt?> GetPromptByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await Context.AiPrompts.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<AiPrompt> AddPromptAsync(AiPrompt prompt, CancellationToken cancellationToken = default)
    {
        await Context.AiPrompts.AddAsync(prompt, cancellationToken);
        return prompt;
    }

    public Task DeletePromptAsync(AiPrompt prompt, CancellationToken cancellationToken = default)
    {
        Context.AiPrompts.Remove(prompt);
        return Task.CompletedTask;
    }
}
