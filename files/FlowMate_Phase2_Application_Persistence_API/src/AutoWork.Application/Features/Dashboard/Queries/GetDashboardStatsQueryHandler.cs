using AutoMapper;
using AutoWork.Application.Common.Exceptions;
using AutoWork.Application.DTOs.Dashboard;
using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Application.Interfaces.Services;
using MediatR;

namespace AutoWork.Application.Features.Dashboard.Queries;

public class GetDashboardStatsQueryHandler : IRequestHandler<GetDashboardStatsQuery, DashboardStatsDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public GetDashboardStatsQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<DashboardStatsDto> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is not Guid userId)
        {
            throw new UnauthorizedException("User is not authenticated.");
        }

        var posts = await _unitOfWork.Posts.GetByUserIdAsync(userId, cancellationToken);
        var postsToday = await _unitOfWork.Posts.CountPublishedTodayAsync(userId, cancellationToken);
        var aiGenerations = await _unitOfWork.Ai.CountGenerationsByUserIdAsync(userId, cancellationToken);
        var credit = await _unitOfWork.Credits.GetByUserIdAsync(userId, cancellationToken);
        var projects = await _unitOfWork.Projects.CountByUserIdAsync(userId, cancellationToken);
        var upcomingSchedules = await _unitOfWork.Schedules.GetUpcomingByUserIdAsync(userId, 100, cancellationToken);
        var auditLogs = await _unitOfWork.AuditLogs.GetRecentByUserIdAsync(userId, 10, cancellationToken);

        var postsChart = Enumerable.Range(0, 7)
            .Select(i =>
            {
                var date = DateTime.UtcNow.Date.AddDays(-6 + i);
                var count = posts.Count(p => p.CreatedAt.Date == date);
                return new ChartDataDto { Label = date.ToString("MM/dd"), Value = count };
            })
            .ToList();

        var aiUsageChart = Enumerable.Range(0, 7)
            .Select(i =>
            {
                var date = DateTime.UtcNow.Date.AddDays(-6 + i);
                return new ChartDataDto { Label = date.ToString("MM/dd"), Value = 0 };
            })
            .ToList();

        return new DashboardStatsDto
        {
            TotalPosts = posts.Count,
            PostsToday = postsToday,
            AiGenerations = aiGenerations,
            CreditsRemaining = credit?.Balance ?? 0,
            ActiveProjects = projects,
            ScheduledPosts = upcomingSchedules.Count,
            PostsChart = postsChart,
            AiUsageChart = aiUsageChart,
            RecentActivities = _mapper.Map<IReadOnlyList<RecentActivityDto>>(auditLogs)
        };
    }
}
