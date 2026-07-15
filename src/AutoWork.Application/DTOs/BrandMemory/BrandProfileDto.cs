namespace AutoWork.Application.DTOs.BrandMemory;

public class BrandProfileDto
{
    public Guid? Id { get; set; }
    public Guid ProjectId { get; set; }
    public bool Exists { get; set; }

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
    public string? VoiceSampleText { get; set; }

    public List<LookupItemDto> Styles { get; set; } = [];
    public List<LookupItemDto> Keywords { get; set; } = [];
    public List<LookupItemDto> Ctas { get; set; } = [];
    public List<LookupItemDto> Hashtags { get; set; } = [];
    public List<BrandColorDto> Colors { get; set; } = [];
    public List<BrandFontDto> Fonts { get; set; } = [];
    public List<ProductCategoryDto> ProductCategories { get; set; } = [];

    /// <summary>Kênh bán hàng — lấy trực tiếp từ ChannelAccounts đã kết nối của project này,
    /// không lưu trùng lặp ở Brand Memory (tránh 2 nguồn sự thật cho cùng 1 dữ liệu).</summary>
    public List<SalesChannelSummaryDto> SalesChannels { get; set; } = [];

    public int CompletedSections { get; set; }
    public int TotalSections { get; set; } = 5;
    public int CompletionPercent { get; set; }
}

public class SalesChannelSummaryDto
{
    public Guid ChannelAccountId { get; set; }
    public string ChannelCode { get; set; } = string.Empty;
    public string ChannelName { get; set; } = string.Empty;
    public string? AccountHandle { get; set; }
    public bool IsConnected { get; set; }
}
