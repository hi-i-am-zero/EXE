using AutoWork.Application.DTOs.Plans;
using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Application.Interfaces.Services;
using AutoWork.Domain.Entities;
using AutoWork.Shared.Enums;
using AutoWork.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoWork.API.Controllers;

[Route("api/plans")]
public class PlansController : ApiControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public PlansController(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<List<PlanDto>>>> GetPlans()
    {
        var plans = await _unitOfWork.Plans.GetActivePlansAsync();
        return OkResponse(plans.Select(MapPlan).ToList());
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<PlanDto>>> GetPlan(Guid id)
    {
        var plan = await _unitOfWork.Plans.GetByIdAsync(id)
            ?? throw new Application.Common.Exceptions.NotFoundException("Plan", id);
        return OkResponse(MapPlan(plan));
    }

    [HttpGet("subscriptions")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<SubscriptionDto?>>> GetCurrentSubscription()
    {
        var subscription = await _unitOfWork.Plans.GetActiveSubscriptionAsync(_currentUser.UserId!.Value);
        return OkResponse(subscription is null ? null : MapSubscription(subscription));
    }

    [HttpPost("subscriptions")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<SubscriptionDto>>> Subscribe([FromBody] CreateSubscriptionRequest request)
    {
        var plan = await _unitOfWork.Plans.GetByIdAsync(request.PlanId)
            ?? throw new Application.Common.Exceptions.NotFoundException("Plan", request.PlanId);

        var existing = await _unitOfWork.Plans.GetActiveSubscriptionAsync(_currentUser.UserId!.Value);
        if (existing is not null)
        {
            existing.Status = (int)SubscriptionStatus.Cancelled;
            existing.CancelledAt = DateTime.UtcNow;
            await _unitOfWork.Plans.UpdateSubscriptionAsync(existing);
        }

        var subscription = await _unitOfWork.Plans.AddSubscriptionAsync(new Subscription
        {
            UserId = _currentUser.UserId!.Value,
            PlanId = plan.Id,
            Status = (int)SubscriptionStatus.Active,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(1),
            AutoRenew = request.AutoRenew
        });

        await _unitOfWork.SaveChangesAsync();
        subscription.Plan = plan;
        return OkResponse(MapSubscription(subscription), "Subscription activated.");
    }

    [HttpDelete("subscriptions")]
    [Authorize]
    public async Task<ActionResult<ApiResponse>> CancelSubscription()
    {
        var subscription = await _unitOfWork.Plans.GetActiveSubscriptionAsync(_currentUser.UserId!.Value)
            ?? throw new Application.Common.Exceptions.NotFoundException("Subscription", "active");

        subscription.Status = (int)SubscriptionStatus.Cancelled;
        subscription.CancelledAt = DateTime.UtcNow;
        subscription.AutoRenew = false;
        await _unitOfWork.Plans.UpdateSubscriptionAsync(subscription);
        await _unitOfWork.SaveChangesAsync();
        return OkResponse("Subscription cancelled.");
    }

    private static PlanDto MapPlan(Plan plan) => new()
    {
        Id = plan.Id,
        Name = plan.Name,
        Description = plan.Description,
        Price = plan.Price,
        BillingPeriod = plan.BillingPeriod,
        MaxProjects = plan.MaxProjects,
        MaxChannels = plan.MaxChannels,
        MaxPostsPerMonth = plan.MaxPostsPerMonth,
        CreditsIncluded = plan.CreditsIncluded,
        IsActive = plan.IsActive,
        SortOrder = plan.SortOrder
    };

    private static SubscriptionDto MapSubscription(Subscription subscription) => new()
    {
        Id = subscription.Id,
        UserId = subscription.UserId,
        PlanId = subscription.PlanId,
        PlanName = subscription.Plan?.Name ?? string.Empty,
        Status = subscription.Status,
        StartDate = subscription.StartDate,
        EndDate = subscription.EndDate,
        CancelledAt = subscription.CancelledAt,
        AutoRenew = subscription.AutoRenew
    };
}
