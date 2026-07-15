using AutoWork.Application.DTOs.BrandMemory;
using AutoWork.Domain.Entities;

namespace AutoWork.Application.Common.Mappings;

/// <summary>
/// Map BrandProfile (kèm các collection con đã Include) sang BrandProfileDto.
/// Đặt thành helper tĩnh dùng chung giữa Query (đọc) và Command (trả về sau khi ghi)
/// thay vì lặp lại đoạn map ~40 dòng ở cả 2 nơi.
/// </summary>
public static class BrandProfileMapper
{
    public static BrandProfileDto ToDto(BrandProfile profile, IReadOnlyList<ChannelAccount> salesChannels)
    {
        var dto = new BrandProfileDto
        {
            Id = profile.Id,
            ProjectId = profile.ProjectId,
            Exists = true,
            LogoUrl = profile.LogoUrl,
            BusinessName = profile.BusinessName,
            BrandName = profile.BrandName,
            Industry = profile.Industry,
            FoundingYear = profile.FoundingYear,
            ShortDescription = profile.ShortDescription,
            Address = profile.Address,
            ContactPhone = profile.ContactPhone,
            ContactEmail = profile.ContactEmail,
            Website = profile.Website,
            VoiceSampleId = profile.VoiceSampleId,
            VoiceSampleText = profile.VoiceSample?.SampleText,
            Styles = profile.BrandProfileStyles
                .Select(x => new LookupItemDto { Id = x.StyleId, Name = x.Style.StyleName })
                .ToList(),
            Keywords = profile.BrandProfileKeywords
                .Select(x => new LookupItemDto { Id = x.ToneKeywordId, Name = x.ToneKeyword.Keyword })
                .ToList(),
            Ctas = profile.BrandCtas
                .Select(x => new LookupItemDto { Id = x.CtaId, Name = x.Cta.CtaText })
                .ToList(),
            Hashtags = profile.BrandHashtags
                .Select(x => new LookupItemDto { Id = x.HashtagId, Name = x.Hashtag.Tag })
                .ToList(),
            Colors = profile.Colors
                .OrderBy(c => c.SortOrder)
                .Select(c => new BrandColorDto { Id = c.Id, ColorHex = c.ColorHex, ColorType = c.ColorType, SortOrder = c.SortOrder })
                .ToList(),
            Fonts = profile.Fonts
                .Select(f => new BrandFontDto { Id = f.Id, FontName = f.FontName, UsageType = f.UsageType })
                .ToList(),
            ProductCategories = profile.ProductCategories
                .Select(c => new ProductCategoryDto
                {
                    Id = c.Id,
                    CategoryName = c.CategoryName,
                    ProductCount = c.Products.Count
                })
                .ToList(),
            SalesChannels = salesChannels
                .Select(sc => new SalesChannelSummaryDto
                {
                    ChannelAccountId = sc.Id,
                    ChannelCode = sc.Channel.Code,
                    ChannelName = sc.Channel.Name,
                    AccountHandle = sc.Name,
                    IsConnected = sc.IsActive
                })
                .ToList()
        };

        CalculateCompletion(dto);
        return dto;
    }

    /// <summary>DTO rỗng khi Project chưa có Brand Memory nào — để FE vẫn render được wizard từ đầu.</summary>
    public static BrandProfileDto Empty(Guid projectId) => new()
    {
        Id = null,
        ProjectId = projectId,
        Exists = false,
        CompletedSections = 0,
        TotalSections = 5,
        CompletionPercent = 0
    };

    private static void CalculateCompletion(BrandProfileDto dto)
    {
        var checks = new[]
        {
            !string.IsNullOrWhiteSpace(dto.BusinessName) && !string.IsNullOrWhiteSpace(dto.BrandName),
            !string.IsNullOrWhiteSpace(dto.Address) || !string.IsNullOrWhiteSpace(dto.ContactPhone) || !string.IsNullOrWhiteSpace(dto.ContactEmail),
            dto.SalesChannels.Count > 0,
            dto.ProductCategories.Count > 0,
            dto.Styles.Count > 0 || dto.Keywords.Count > 0 || dto.Colors.Count > 0
        };

        dto.CompletedSections = checks.Count(c => c);
        dto.TotalSections = checks.Length;
        dto.CompletionPercent = (int)Math.Round(dto.CompletedSections * 100.0 / checks.Length, MidpointRounding.AwayFromZero);
    }
}
