namespace AutoWork.Application.DTOs.Plans;

public class PlanDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int BillingPeriod { get; set; }
    public int MaxProjects { get; set; }
    public int MaxChannels { get; set; }
    public int MaxPostsPerMonth { get; set; }
    public int CreditsIncluded { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
}
