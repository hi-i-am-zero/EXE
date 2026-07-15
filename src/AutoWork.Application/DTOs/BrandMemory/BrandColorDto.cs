namespace AutoWork.Application.DTOs.BrandMemory;

public class BrandColorDto
{
    public Guid Id { get; set; }
    public string ColorHex { get; set; } = string.Empty;
    public string ColorType { get; set; } = "Secondary";
    public int SortOrder { get; set; }
}
