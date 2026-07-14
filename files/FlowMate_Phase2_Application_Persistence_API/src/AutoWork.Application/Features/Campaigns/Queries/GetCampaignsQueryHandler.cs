using AutoWork.Application.Common.Exceptions;
using AutoWork.Application.DTOs.Campaigns;
using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Application.Interfaces.Services;
using AutoWork.Domain.Entities;
using MediatR;

namespace AutoWork.Application.Features.Campaigns.Queries;

public class GetCampaignsQueryHandler : IRequestHandler<GetCampaignsQuery, List<CampaignDto>>
{
    private readonly IRepository<Campaign> _campaigns;
    private readonly IRepository<CampaignGoal> _goals;
    private readonly IRepository<PromotionType> _promoTypes;
    private readonly IProjectAccessGuard _accessGuard;
    private readonly ICurrentUserService _currentUser;

    public GetCampaignsQueryHandler(
        IRepository<Campaign> campaigns,
        IRepository<CampaignGoal> goals,
        IRepository<PromotionType> promoTypes,
        IProjectAccessGuard accessGuard,
        ICurrentUserService currentUser)
    {
        _campaigns = campaigns;
        _goals = goals;
        _promoTypes = promoTypes;
        _accessGuard = accessGuard;
        _currentUser = currentUser;
    }

    public async Task<List<CampaignDto>> Handle(GetCampaignsQuery request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not Guid userId)
        {
            throw new UnauthorizedException("User is not authenticated.");
        }

        await _accessGuard.EnsureViewerAsync(request.ProjectId, userId, cancellationToken);

        var campaigns = await _campaigns.FindAsync(c => c.ProjectId == request.ProjectId, cancellationToken);
        var goals = (await _goals.GetAllAsync(cancellationToken)).ToDictionary(g => g.Id, g => g.GoalName);
        var promoTypes = (await _promoTypes.GetAllAsync(cancellationToken)).ToDictionary(t => t.Id, t => t.TypeName);

        return campaigns
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new CampaignDto
            {
                Id = c.Id,
                ProjectId = c.ProjectId,
                Name = c.Name,
                GoalName = goals.GetValueOrDefault(c.GoalId, string.Empty),
                PromotionTypeName = promoTypes.GetValueOrDefault(c.PromotionTypeId, string.Empty),
                DiscountPercent = c.DiscountPercent,
                DiscountAmount = c.DiscountAmount,
                MinOrderAmount = c.MinOrderAmount,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                NumberOfPosts = c.NumberOfPosts,
                Status = c.Status,
                CreatedAt = c.CreatedAt
            })
            .ToList();
    }
}
