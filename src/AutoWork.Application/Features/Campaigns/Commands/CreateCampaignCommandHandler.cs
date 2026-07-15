using AutoWork.Application.Common.Exceptions;
using AutoWork.Application.DTOs.Campaigns;
using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Application.Interfaces.Services;
using AutoWork.Domain.Entities;
using MediatR;

namespace AutoWork.Application.Features.Campaigns.Commands;

public class CreateCampaignCommandHandler : IRequestHandler<CreateCampaignCommand, CampaignDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<Campaign> _campaigns;
    private readonly IRepository<CampaignGoal> _goals;
    private readonly IRepository<PromotionType> _promoTypes;
    private readonly IRepository<CampaignChannelAccount> _campaignChannels;
    private readonly IRepository<ChannelAccount> _channelAccounts;
    private readonly IProjectAccessGuard _accessGuard;
    private readonly ICurrentUserService _currentUser;

    public CreateCampaignCommandHandler(
        IUnitOfWork unitOfWork,
        IRepository<Campaign> campaigns,
        IRepository<CampaignGoal> goals,
        IRepository<PromotionType> promoTypes,
        IRepository<CampaignChannelAccount> campaignChannels,
        IRepository<ChannelAccount> channelAccounts,
        IProjectAccessGuard accessGuard,
        ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _campaigns = campaigns;
        _goals = goals;
        _promoTypes = promoTypes;
        _campaignChannels = campaignChannels;
        _channelAccounts = channelAccounts;
        _accessGuard = accessGuard;
        _currentUser = currentUser;
    }

    public async Task<CampaignDto> Handle(CreateCampaignCommand command, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not Guid userId)
        {
            throw new UnauthorizedException("User is not authenticated.");
        }

        await _accessGuard.EnsureEditorAsync(command.Request.ProjectId, userId, cancellationToken);

        var goal = await _goals.GetByIdAsync(command.Request.GoalId, cancellationToken)
            ?? throw new BadRequestException("Mục tiêu chiến dịch không hợp lệ.");
        var promoType = await _promoTypes.GetByIdAsync(command.Request.PromotionTypeId, cancellationToken)
            ?? throw new BadRequestException("Loại ưu đãi không hợp lệ.");

        var campaign = new Campaign
        {
            ProjectId = command.Request.ProjectId,
            Name = command.Request.Name.Trim(),
            GoalId = command.Request.GoalId,
            PromotionTypeId = command.Request.PromotionTypeId,
            DiscountPercent = command.Request.DiscountPercent,
            DiscountAmount = command.Request.DiscountAmount,
            MinOrderAmount = command.Request.MinOrderAmount,
            StartDate = command.Request.StartDate,
            EndDate = command.Request.EndDate,
            NumberOfPosts = command.Request.NumberOfPosts,
            Status = 0 // CampaignStatus.Draft
        };

        await _campaigns.AddAsync(campaign, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (command.Request.ChannelAccountIds.Count > 0)
        {
            var validChannelIds = (await _channelAccounts.FindAsync(
                ca => ca.ProjectId == command.Request.ProjectId && command.Request.ChannelAccountIds.Contains(ca.Id),
                cancellationToken)).Select(ca => ca.Id).ToHashSet();

            foreach (var channelAccountId in command.Request.ChannelAccountIds.Where(validChannelIds.Contains))
            {
                await _campaignChannels.AddAsync(
                    new CampaignChannelAccount { CampaignId = campaign.Id, ChannelAccountId = channelAccountId },
                    cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return new CampaignDto
        {
            Id = campaign.Id,
            ProjectId = campaign.ProjectId,
            Name = campaign.Name,
            GoalName = goal.GoalName,
            PromotionTypeName = promoType.TypeName,
            DiscountPercent = campaign.DiscountPercent,
            DiscountAmount = campaign.DiscountAmount,
            MinOrderAmount = campaign.MinOrderAmount,
            StartDate = campaign.StartDate,
            EndDate = campaign.EndDate,
            NumberOfPosts = campaign.NumberOfPosts,
            Status = campaign.Status,
            CreatedAt = campaign.CreatedAt
        };
    }
}
