using AutoWork.Domain.Entities;

namespace AutoWork.Application.Common.Helpers;

/// <summary>
/// Tự động nối "thông tin cố định" (tên shop, liên hệ) vào cuối content mỗi bài viết — đúng luồng
/// mô tả: "ở dưới mỗi content của bài nó sẽ có thêm thông tin cố định mà bài nào cũng có như tên
/// shop và thông tin liên hệ". Dùng chung cho cả bài viết đơn (CreatePostCommand) và bài viết do AI
/// sinh trong Timeline (ConfirmTimelineOptionCommand).
/// </summary>
public static class PostContentFooterBuilder
{
    public static string AppendBrandFooter(string content, BrandProfile? brandProfile)
    {
        if (brandProfile is null)
        {
            return content;
        }

        var footerLines = new List<string> { "———", brandProfile.BrandName };

        var contactParts = new List<string>();
        if (!string.IsNullOrWhiteSpace(brandProfile.ContactPhone)) contactParts.Add($"📞 {brandProfile.ContactPhone}");
        if (!string.IsNullOrWhiteSpace(brandProfile.Website)) contactParts.Add($"🌐 {brandProfile.Website}");
        if (contactParts.Count > 0)
        {
            footerLines.Add(string.Join("  |  ", contactParts));
        }

        return $"{content}\n\n{string.Join("\n", footerLines)}";
    }
}
