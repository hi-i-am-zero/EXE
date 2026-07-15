namespace AutoWork.Application.DTOs.Dashboard;

public class DashboardStatsDto
{
    public int TotalPosts { get; set; }
    public int PostsToday { get; set; }
    public int AiGenerations { get; set; }
    public int CreditsRemaining { get; set; }
    public int ActiveProjects { get; set; }
    public int ScheduledPosts { get; set; }
    public IReadOnlyList<ChartDataDto> PostsChart { get; set; } = [];
    public IReadOnlyList<ChartDataDto> AiUsageChart { get; set; } = [];
    public IReadOnlyList<RecentActivityDto> RecentActivities { get; set; } = [];
}
