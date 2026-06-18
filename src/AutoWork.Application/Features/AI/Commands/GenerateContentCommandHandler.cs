using AutoWork.Application.Common.Exceptions;
using AutoWork.Application.DTOs.AI;
using AutoWork.Application.Interfaces.Services;
using MediatR;

namespace AutoWork.Application.Features.AI.Commands;

public class GenerateContentCommandHandler : IRequestHandler<GenerateContentCommand, GenerateContentResponse>
{
    private const int CreditCost = 1;
    private readonly IAiContentService _aiContentService;
    private readonly ICreditService _creditService;
    private readonly ICurrentUserService _currentUserService;

    public GenerateContentCommandHandler(
        IAiContentService aiContentService,
        ICreditService creditService,
        ICurrentUserService currentUserService)
    {
        _aiContentService = aiContentService;
        _creditService = creditService;
        _currentUserService = currentUserService;
    }

    public async Task<GenerateContentResponse> Handle(GenerateContentCommand command, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is not Guid userId)
        {
            throw new UnauthorizedException("User is not authenticated.");
        }

        if (!await _creditService.HasSufficientCreditsAsync(userId, CreditCost, cancellationToken))
        {
            throw new BadRequestException("Insufficient credits to generate content.");
        }

        var response = await _aiContentService.GenerateContentAsync(userId, command.Request, cancellationToken);

        await _creditService.DeductCreditsAsync(
            userId,
            CreditCost,
            Shared.Enums.CreditTransactionType.GenerateContent,
            $"AI content generation: {command.Request.Topic}",
            nameof(GenerateContentResponse),
            response.Id,
            cancellationToken);

        return response;
    }
}
