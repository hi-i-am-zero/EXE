namespace AutoWork.Application.DTOs.BrandMemory;

public class ProductCategoryDto
{
    public Guid Id { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int ProductCount { get; set; }
}
