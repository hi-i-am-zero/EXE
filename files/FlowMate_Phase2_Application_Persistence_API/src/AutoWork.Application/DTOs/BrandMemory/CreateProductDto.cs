namespace AutoWork.Application.DTOs.BrandMemory;

public class CreateProductDto
{
    public Guid ProjectId { get; set; }
    public Guid? ProductCategoryId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal? Price { get; set; }

    /// <summary>Id các MediaFile đã upload trước đó (qua MediaController có sẵn) để gắn làm ảnh sản phẩm.</summary>
    public List<Guid> MediaFileIds { get; set; } = [];
}
