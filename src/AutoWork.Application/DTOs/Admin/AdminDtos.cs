namespace AutoWork.Application.DTOs.Admin;

public class AdminDashboardDto
{
    public int TotalUsers { get; set; }
    public int TotalPosts { get; set; }
    public int TotalPayments { get; set; }
    public decimal TotalRevenue { get; set; }
}

public class UpdateUserStatusRequest
{
    public bool IsActive { get; set; }
}
