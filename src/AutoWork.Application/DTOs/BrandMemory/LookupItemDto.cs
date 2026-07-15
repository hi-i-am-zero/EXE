namespace AutoWork.Application.DTOs.BrandMemory;

/// <summary>Item lookup dùng chung: BrandStyle/ToneKeyword/CtaTemplate/Hashtag/VoiceSampleTemplate.</summary>
public class LookupItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
