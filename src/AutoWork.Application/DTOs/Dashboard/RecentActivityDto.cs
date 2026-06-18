namespace AutoWork.Application.DTOs.Dashboard;

public class RecentActivityDto
{
    public Guid Id { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}
