using AutoWork.Application.Common.Exceptions;
using AutoWork.Application.DTOs.AI;
using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Application.Interfaces.Services;
using AutoWork.Domain.Entities;
using MediatR;

namespace AutoWork.Application.Features.Campaigns.Commands;

/// <summary>Bước 2 của luồng tạo Chiến dịch tự động: upload ảnh sản phẩm -> AI sinh mô tả chung
/// -> lưu vào Campaign.ProductDescription để bước 3 (sinh 3 phương án timeline) dùng làm ngữ cảnh.</summary>
public class GenerateCampaignProductDescriptionCommandHandler
    : IRequestHandler<GenerateCampaignProductDescriptionCommand, GenerateCampaignProductDescriptionResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<Campaign> _campaigns;
    private readonly IProductVisionService _visionService;
    private readonly IProjectAccessGuard _accessGuard;
    private readonly ICurrentUserService _currentUser;

    public GenerateCampaignProductDescriptionCommandHandler(
        IUnitOfWork unitOfWork,
        IRepository<Campaign> campaigns,
        IProductVisionService visionService,
        IProjectAccessGuard accessGuard,
        ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _campaigns = campaigns;
        _visionService = visionService;
        _accessGuard = accessGuard;
        _currentUser = currentUser;
    }

    public async Task<GenerateCampaignProductDescriptionResponse> Handle(
        GenerateCampaignProductDescriptionCommand command, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not Guid userId)
        {
            throw new UnauthorizedException("User is not authenticated.");
        }

        var campaign = await _campaigns.GetByIdAsync(command.Request.CampaignId, cancellationToken)
            ?? throw new NotFoundException(nameof(Campaign), command.Request.CampaignId);

        await _accessGuard.EnsureEditorAsync(campaign.ProjectId, userId, cancellationToken);

        var (description, creditsUsed, tokensUsed) = await _visionService.GenerateDescriptionFromImagesAsync(
            userId, command.Request.MediaFileIds, command.Request.Provider, cancellationToken);

        campaign.ProductDescription = description;
        await _campaigns.UpdateAsync(campaign, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new GenerateCampaignProductDescriptionResponse
        {
            CampaignId = campaign.Id,
            Description = description,
            CreditsUsed = creditsUsed,
            TokensUsed = tokensUsed
        };
    }
}
