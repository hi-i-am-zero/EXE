namespace AutoWork.Application.DTOs.BrandMemory;

/// <summary>Toàn bộ danh mục lookup để render dropdown/checkbox ở màn Brand Memory.</summary>
public class BrandMemoryLookupOptionsDto
{
    public List<LookupItemDto> Styles { get; set; } = [];
    public List<LookupItemDto> ToneKeywords { get; set; } = [];
    public List<LookupItemDto> CtaTemplates { get; set; } = [];
    public List<LookupItemDto> Hashtags { get; set; } = [];
    public List<VoiceSampleDto> VoiceSamples { get; set; } = [];
}

public class VoiceSampleDto
{
    public Guid Id { get; set; }
    public string SampleText { get; set; } = string.Empty;
}
