using AutoWork.Application.Common.Exceptions;
using AutoWork.Application.DTOs.AI;
using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Application.Interfaces.Services;
using AutoWork.Domain.Entities;
using AutoWork.Domain.Enums;
using AutoWork.Shared.Constants;
using AutoWork.Shared.Enums;
using Microsoft.Extensions.Logging;

namespace AutoWork.Infrastructure.AI;

public class AiContentService : IAiContentService
{
    private const string SystemPrompt = """
        You are a professional marketing content writer for Vietnamese and international audiences.
        Always respond with valid JSON using this schema:
        {
          "title": "string",
          "description": "string",
          "content": "string",
          "hashtags": "string"
        }
        """;

    private readonly AiProviderFactory _providerFactory;
    private readonly ICreditService _creditService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AiContentService> _logger;

    public AiContentService(
        AiProviderFactory providerFactory,
        ICreditService creditService,
        IUnitOfWork unitOfWork,
        ILogger<AiContentService> logger)
    {
        _providerFactory = providerFactory;
        _creditService = creditService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<GenerateContentResponse> GenerateContentAsync(
        Guid userId,
        GenerateContentRequest request,
        CancellationToken cancellationToken = default)
    {
        var creditCost = CreditCosts.GenerateContent;
        if (!await _creditService.HasSufficientCreditsAsync(userId, creditCost, cancellationToken))
        {
            throw new BadRequestException("Insufficient credits for AI content generation.");
        }

        var promptTemplate = await ResolvePromptTemplateAsync(request.PromptId, cancellationToken);
        var userPrompt = BuildUserPrompt(request, promptTemplate);
        var provider = _providerFactory.GetProvider(request.Provider);

        var aiPrompt = new AiPrompt
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = $"Generation - {request.Topic}",
            Template = userPrompt,
            Category = "generated",
            IsSystem = false,
            CreatedAt = DateTime.UtcNow
        };
        await _unitOfWork.Ai.AddPromptAsync(aiPrompt, cancellationToken);

        var generated = new AiGeneratedContent
        {
            Id = Guid.NewGuid(),
            AiPromptId = aiPrompt.Id,
            UserId = userId,
            Input = userPrompt,
            Status = (int)AiContentStatus.Processing,
            CreatedAt = DateTime.UtcNow
        };
        await _unitOfWork.Ai.AddAsync(generated, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            var result = await provider.GenerateAsync(new AiGenerationRequest
            {
                SystemPrompt = SystemPrompt,
                UserPrompt = userPrompt,
                MaxTokens = Math.Max(request.Length * 2, 1024)
            }, cancellationToken);

            generated.Output = result.RawContent;
            generated.TokensUsed = result.TokensUsed;
            generated.Status = (int)AiContentStatus.Completed;
            generated.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Ai.UpdateAsync(generated, cancellationToken);

            await _creditService.DeductCreditsAsync(
                userId,
                creditCost,
                Shared.Enums.CreditTransactionType.GenerateContent,
                $"AI content generation: {request.Topic}",
                nameof(AiGeneratedContent),
                generated.Id,
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new GenerateContentResponse
            {
                Id = generated.Id,
                Title = result.Title,
                Description = result.Description,
                Content = result.Content,
                Hashtags = result.Hashtags,
                CreditsUsed = creditCost,
                TokensUsed = result.TokensUsed
            };
        }
        catch (Exception ex)
        {
            generated.Status = (int)AiContentStatus.Failed;
            generated.ErrorMessage = ex.Message;
            generated.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Ai.UpdateAsync(generated, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogError(ex, "AI content generation failed for user {UserId}", userId);
            throw;
        }
    }

    private async Task<string?> ResolvePromptTemplateAsync(Guid? promptId, CancellationToken cancellationToken)
    {
        if (!promptId.HasValue)
        {
            return null;
        }

        var prompt = await _unitOfWork.Ai.GetPromptByIdAsync(promptId.Value, cancellationToken);
        return prompt?.Template;
    }

    private static string BuildUserPrompt(GenerateContentRequest request, string? template)
    {
        if (!string.IsNullOrWhiteSpace(template))
        {
            return template
                .Replace("{{topic}}", request.Topic, StringComparison.OrdinalIgnoreCase)
                .Replace("{{keywords}}", request.Keywords ?? string.Empty, StringComparison.OrdinalIgnoreCase)
                .Replace("{{length}}", request.Length.ToString(), StringComparison.OrdinalIgnoreCase)
                .Replace("{{tone}}", request.Tone ?? "professional", StringComparison.OrdinalIgnoreCase)
                .Replace("{{industry}}", request.Industry ?? string.Empty, StringComparison.OrdinalIgnoreCase)
                .Replace("{{cta}}", request.Cta ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        return $"""
            Create marketing content with the following requirements:
            - Topic: {request.Topic}
            - Keywords: {request.Keywords ?? "N/A"}
            - Target length: approximately {request.Length} words
            - Tone: {request.Tone ?? "professional"}
            - Industry: {request.Industry ?? "general"}
            - Call to action: {request.Cta ?? "N/A"}
            """;
    }
}
