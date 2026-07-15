namespace AutoWork.Application.DTOs.BrandMemory;

/// <summary>Section 1 + 2: Thông tin doanh nghiệp + Địa chỉ liên hệ.</summary>
public class UpsertBrandProfileCoreDto
{
    public Guid ProjectId { get; set; }

    public string? LogoUrl { get; set; }
    public string BusinessName { get; set; } = string.Empty;
    public string BrandName { get; set; } = string.Empty;
    public string? Industry { get; set; }
    public short? FoundingYear { get; set; }
    public string? ShortDescription { get; set; }

    public string? Address { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }
    public string? Website { get; set; }

    public Guid? VoiceSampleId { get; set; }
}
