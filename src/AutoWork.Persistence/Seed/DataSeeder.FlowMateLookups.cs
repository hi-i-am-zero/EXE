using AutoWork.Domain.Entities;
using AutoWork.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace AutoWork.Persistence.Seed;

/// <summary>
/// Seed dữ liệu lookup cho các bảng mới của FlowMate v2 (Brand Memory, Campaign, Timeline).
/// Tách thành partial class riêng để không làm phình to DataSeeder.cs gốc — mỗi domain area
/// có 1 file seed riêng, dễ tìm và dễ review khi có PR.
/// </summary>
public static partial class DataSeeder
{
    private static async Task SeedFlowMateLookupsAsync(ApplicationDbContext context, CancellationToken cancellationToken)
    {
        await SeedBrandStylesAsync(context, cancellationToken);
        await SeedToneKeywordsAsync(context, cancellationToken);
        await SeedVoiceSampleTemplatesAsync(context, cancellationToken);
        await SeedCtaTemplatesAsync(context, cancellationToken);
        await SeedCampaignGoalsAsync(context, cancellationToken);
        await SeedPromotionTypesAsync(context, cancellationToken);
        await SeedTimelineTemplateTypesAsync(context, cancellationToken);
    }

    private static async Task SeedBrandStylesAsync(ApplicationDbContext context, CancellationToken cancellationToken)
    {
        if (await context.BrandStyles.AnyAsync(cancellationToken)) return;

        string[] names =
        [
            "Streetwear", "Y2K", "Basic", "Nữ tính", "Công sở", "Vintage", "Boho",
            "Sang trọng", "Thể thao", "Dạo phố", "Tomboy", "Dễ thương", "Unisex", "Local brand"
        ];
        context.BrandStyles.AddRange(names.Select(n => new BrandStyle { StyleName = n }));
    }

    private static async Task SeedToneKeywordsAsync(ApplicationDbContext context, CancellationToken cancellationToken)
    {
        if (await context.ToneKeywords.AnyAsync(cancellationToken)) return;

        string[] keywords =
        [
            "Gần gũi", "Dí dỏm", "Trẻ trung", "Sang trọng", "Nghiêm túc", "Trưởng thành",
            "Nhiệt tình", "Điềm tĩnh", "Táo bạo", "Kín đáo", "Chân thành", "Quyết đoán",
            "Ấm áp", "Tối giản", "Bí ẩn"
        ];
        context.ToneKeywords.AddRange(keywords.Select(k => new ToneKeyword { Keyword = k }));
    }

    private static async Task SeedVoiceSampleTemplatesAsync(ApplicationDbContext context, CancellationToken cancellationToken)
    {
        if (await context.VoiceSampleTemplates.AnyAsync(cancellationToken)) return;

        string[] samples =
        [
            "Hàng mới về nè cả nhà ơi, xinh xỉu luôn á 🐱",
            "Bộ sưu tập mới đã có mặt tại cửa hàng.",
            "Chào đón thiết kế mới — tinh giản, tinh tế, dành cho phái đẹp hiện đại.",
            "Ê mấy bạn, đợt này về hàng chất lắm, lướt xuống coi liền tay nè!",
            "Item của tuần: form dáng chuẩn, chất liệu bền, phối được với mọi outfit.",
            "Một thiết kế, nhiều câu chuyện. Khám phá bộ sưu tập giới hạn ngay hôm nay."
        ];
        context.VoiceSampleTemplates.AddRange(samples.Select(s => new VoiceSampleTemplate { SampleText = s }));
    }

    private static async Task SeedCtaTemplatesAsync(ApplicationDbContext context, CancellationToken cancellationToken)
    {
        if (await context.CtaTemplates.AnyAsync(cancellationToken)) return;

        string[] ctas = ["Chốt đơn ngay", "Inbox tư vấn", "Đặt hàng liền tay"];
        context.CtaTemplates.AddRange(ctas.Select(c => new CtaTemplate { CtaText = c }));
    }

    private static async Task SeedCampaignGoalsAsync(ApplicationDbContext context, CancellationToken cancellationToken)
    {
        if (await context.CampaignGoals.AnyAsync(cancellationToken)) return;

        string[] goals =
        [
            "Ra mắt sản phẩm mới", "Bán bộ sưu tập theo mùa", "Xả hàng tồn kho",
            "Flash sale chớp nhoáng", "Tăng nhận diện thương hiệu", "Tăng tương tác - follow",
            "Giữ chân khách cũ", "Mừng dịp lễ - sự kiện"
        ];
        context.CampaignGoals.AddRange(goals.Select(g => new CampaignGoal { GoalName = g }));
    }

    private static async Task SeedPromotionTypesAsync(ApplicationDbContext context, CancellationToken cancellationToken)
    {
        if (await context.PromotionTypes.AnyAsync(cancellationToken)) return;

        string[] types =
        [
            "Giảm %", "Giảm số tiền cố định", "Mua X tặng Y", "Freeship",
            "Quà tặng kèm", "Giảm giá theo combo", "Mã voucher riêng", "Không có ưu đãi"
        ];
        context.PromotionTypes.AddRange(types.Select(t => new PromotionType { TypeName = t }));
    }

    private static async Task SeedTimelineTemplateTypesAsync(ApplicationDbContext context, CancellationToken cancellationToken)
    {
        if (await context.TimelineTemplateTypes.AnyAsync(cancellationToken)) return;

        context.TimelineTemplateTypes.AddRange(
            new TimelineTemplateType
            {
                TypeName = "Classic Campaign",
                Description = "Timeline theo từng ngày, phù hợp chiến dịch marketing, ra mắt sản phẩm, khuyến mãi."
            },
            new TimelineTemplateType
            {
                TypeName = "Roadmap",
                Description = "Timeline theo từng giai đoạn, phù hợp kế hoạch dài hạn, roadmap dự án."
            },
            new TimelineTemplateType
            {
                TypeName = "Calendar",
                Description = "Timeline theo lịch thực tế, phù hợp lịch sự kiện, deadline, hoạt động theo tháng."
            });
    }
}
