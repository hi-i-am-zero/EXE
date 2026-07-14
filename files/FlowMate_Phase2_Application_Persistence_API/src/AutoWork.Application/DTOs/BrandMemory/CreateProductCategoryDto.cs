namespace AutoWork.Application.DTOs.BrandMemory;

public class CreateProductCategoryDto
{
    public Guid ProjectId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
}
