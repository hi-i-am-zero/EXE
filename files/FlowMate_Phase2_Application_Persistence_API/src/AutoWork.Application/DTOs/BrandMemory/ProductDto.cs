namespace AutoWork.Application.DTOs.BrandMemory;

public class ProductDto
{
    public Guid Id { get; set; }
    public Guid? ProductCategoryId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal? Price { get; set; }
    public string? AiGeneratedDescription { get; set; }
    public List<string> ImageUrls { get; set; } = [];
}
