namespace AutoWork.Application.DTOs.BrandMemory;

public class SetBrandColorsDto
{
    public Guid ProjectId { get; set; }
    public List<BrandColorItemDto> Colors { get; set; } = [];
}

public class BrandColorItemDto
{
    public string ColorHex { get; set; } = string.Empty;
    public string ColorType { get; set; } = "Secondary";
    public int SortOrder { get; set; }
}
