using AutoWork.Application.DTOs.Affiliates;
using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Application.Interfaces.Services;
using AutoWork.Domain.Entities;
using AutoWork.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoWork.API.Controllers;

[Authorize]
public class AffiliatesController : ApiControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public AffiliatesController(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<AffiliateDto>>> GetProfile()
    {
        var affiliate = await _unitOfWork.Affiliates.GetByUserIdAsync(_currentUser.UserId!.Value);
        if (affiliate is null)
        {
            affiliate = new Affiliate
            {
                UserId = _currentUser.UserId!.Value,
                Code = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant(),
                CommissionRate = 0.10m,
                IsActive = true
            };
            await _unitOfWork.Affiliates.AddAsync(affiliate);
            await _unitOfWork.SaveChangesAsync();
        }

        return OkResponse(new AffiliateDto
        {
            Id = affiliate.Id,
            Code = affiliate.Code,
            CommissionRate = affiliate.CommissionRate,
            TotalEarnings = affiliate.TotalEarnings,
            IsActive = affiliate.IsActive
        });
    }

    [HttpGet("links")]
    public async Task<ActionResult<ApiResponse<List<AffiliateLinkDto>>>> GetLinks()
    {
        var affiliate = await _unitOfWork.Affiliates.GetByUserIdAsync(_currentUser.UserId!.Value)
            ?? throw new Application.Common.Exceptions.NotFoundException("Affiliate", _currentUser.UserId!.Value);

        return OkResponse(affiliate.Links.Select(l => new AffiliateLinkDto
        {
            Id = l.Id,
            Url = l.Url,
            Campaign = l.Campaign,
            ClickCount = l.ClickCount,
            ConversionCount = l.ConversionCount
        }).ToList());
    }

    [HttpPost("links")]
    public async Task<ActionResult<ApiResponse<AffiliateLinkDto>>> CreateLink([FromBody] CreateAffiliateLinkRequest request)
    {
        var affiliate = await _unitOfWork.Affiliates.GetByUserIdAsync(_currentUser.UserId!.Value)
            ?? throw new Application.Common.Exceptions.NotFoundException("Affiliate", _currentUser.UserId!.Value);

        var link = new AffiliateLink
        {
            AffiliateId = affiliate.Id,
            Name = request.Campaign ?? "Default",
            Url = $"https://autowork.vn/ref/{affiliate.Code}",
            Campaign = request.Campaign,
            IsActive = true
        };

        affiliate.Links.Add(link);
        await _unitOfWork.SaveChangesAsync();

        return OkResponse(new AffiliateLinkDto
        {
            Id = link.Id,
            Url = link.Url,
            Campaign = link.Campaign
        }, "Affiliate link created.");
    }

    [HttpGet("commissions")]
    public async Task<ActionResult<ApiResponse<List<AffiliateCommissionDto>>>> GetCommissions()
    {
        var affiliate = await _unitOfWork.Affiliates.GetByUserIdAsync(_currentUser.UserId!.Value)
            ?? throw new Application.Common.Exceptions.NotFoundException("Affiliate", _currentUser.UserId!.Value);

        var commissions = await _unitOfWork.Affiliates.GetCommissionsByAffiliateIdAsync(affiliate.Id);
        return OkResponse(commissions.Select(c => new AffiliateCommissionDto
        {
            Id = c.Id,
            Amount = c.Amount,
            Status = c.Status,
            CreatedAt = c.CreatedAt
        }).ToList());
    }
}
