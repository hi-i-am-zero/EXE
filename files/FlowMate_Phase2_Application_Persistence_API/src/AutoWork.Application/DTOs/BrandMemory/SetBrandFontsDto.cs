namespace AutoWork.Application.DTOs.BrandMemory;

public class SetBrandFontsDto
{
    public Guid ProjectId { get; set; }
    public List<BrandFontItemDto> Fonts { get; set; } = [];
}

public class BrandFontItemDto
{
    public string FontName { get; set; } = string.Empty;
    public string UsageType { get; set; } = "Body";
}
