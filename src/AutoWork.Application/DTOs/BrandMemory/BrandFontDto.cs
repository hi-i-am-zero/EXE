namespace AutoWork.Application.DTOs.BrandMemory;

public class BrandFontDto
{
    public Guid Id { get; set; }
    public string FontName { get; set; } = string.Empty;
    public string UsageType { get; set; } = "Body";
}
