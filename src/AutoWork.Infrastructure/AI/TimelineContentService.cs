using System.Text.Json;
using AutoWork.Application.Common.Exceptions;
using AutoWork.Application.Common.Helpers;
using AutoWork.Application.DTOs.AI;
using AutoWork.Application.DTOs.Timelines;
using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Application.Interfaces.Services;
using AutoWork.Domain.Entities;
using AutoWork.Domain.Enums;
using AutoWork.Shared.Constants;
using AutoWork.Shared.Enums;
using Microsoft.Extensions.Logging;

namespace AutoWork.Infrastructure.AI;

public class TimelineContentService : ITimelineContentService
{
    private const string TimelineOptionsSystemPrompt = """
        You are a professional Vietnamese social media marketing strategist. Given a campaign goal,
        product description, brand voice, and a date range, generate THREE DIFFERENT complete posting
        timeline proposals — one in the style of each given "template type" (Classic Campaign = ngày nào
        cũng có bài, đều đặn; Roadmap = chia theo giai đoạn chuẩn bị/ra mắt/quảng bá/tăng trưởng/tổng kết;
        Calendar = bám theo các mốc lịch/sự kiện thực tế trong tháng). Mỗi phương án phải có SỐ BÀI VIẾT
        đúng bằng số bài yêu cầu, KHÔNG được trùng ý tưởng giữa 3 phương án.
        Respond with ONLY a valid JSON object (no markdown, no explanation outside JSON):
        {
          "options": [
            {
              "templateType": "Classic Campaign",
              "strategyDescription": "string (1-2 câu mô tả chiến lược của phương án này)",
              "posts": [
                { "dayOffset": 0, "title": "string", "content": "string (tiếng Việt, có emoji)", "hashtags": ["tag1","tag2"] }
              ]
            },
            { "templateType": "Roadmap", "strategyDescription": "string", "posts": [ ... ] },
            { "templateType": "Calendar", "strategyDescription": "string", "posts": [ ... ] }
          ]
        }
        "dayOffset" là số ngày kể từ ngày bắt đầu chiến dịch (0 = ngày đầu tiên).
        """;

    private readonly AiProviderFactory _providerFactory;
    private readonly ICreditService _creditService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<Campaign> _campaigns;
    private readonly IRepository<CampaignGoal> _goals;
    private readonly IRepository<PromotionType> _promotionTypes;
    private readonly IRepository<BrandProfile> _brandProfiles;
    private readonly IRepository<TimelineTemplateType> _templateTypes;
    private readonly IRepository<Timeline> _timelines;
    private readonly IRepository<Post> _posts;
    private readonly IRepository<PostContent> _postContents;
    private readonly IRepository<Hashtag> _hashtags;
    private readonly IRepository<PostHashtag> _postHashtags;
    private readonly ILogger<TimelineContentService> _logger;

    public TimelineContentService(
        AiProviderFactory providerFactory,
        ICreditService creditService,
        IUnitOfWork unitOfWork,
        IRepository<Campaign> campaigns,
        IRepository<CampaignGoal> goals,
        IRepository<PromotionType> promotionTypes,
        IRepository<BrandProfile> brandProfiles,
        IRepository<TimelineTemplateType> templateTypes,
        IRepository<Timeline> timelines,
        IRepository<Post> posts,
        IRepository<PostContent> postContents,
        IRepository<Hashtag> hashtags,
        IRepository<PostHashtag> postHashtags,
        ILogger<TimelineContentService> logger)
    {
        _providerFactory = providerFactory;
        _creditService = creditService;
        _unitOfWork = unitOfWork;
        _campaigns = campaigns;
        _goals = goals;
        _promotionTypes = promotionTypes;
        _brandProfiles = brandProfiles;
        _templateTypes = templateTypes;
        _timelines = timelines;
        _posts = posts;
        _postContents = postContents;
        _hashtags = hashtags;
        _postHashtags = postHashtags;
        _logger = logger;
    }

    public async Task<GenerateTimelineOptionsResponse> GenerateTimelineOptionsAsync(
        Guid userId, GenerateTimelineOptionsRequest request, CancellationToken cancellationToken = default)
    {
        // Sinh 3 phương án 1 lần nặng hơn tạo 1 nội dung đơn — tính phí gấp đôi cho hợp lý
        var creditCost = CreditCosts.GenerateContent * 2;
        if (!await _creditService.HasSufficientCreditsAsync(userId, creditCost, cancellationToken))
        {
            throw new BadRequestException("Insufficient credits for AI timeline generation.");
        }

        var campaign = await _campaigns.GetByIdAsync(request.CampaignId, cancellationToken)
            ?? throw new NotFoundException(nameof(Campaign), request.CampaignId);

        var goal = await _goals.GetByIdAsync(campaign.GoalId, cancellationToken);
        var promoType = await _promotionTypes.GetByIdAsync(campaign.PromotionTypeId, cancellationToken);
        var brandProfile = (await _brandProfiles.FindAsync(b => b.ProjectId == campaign.ProjectId, cancellationToken)).FirstOrDefault();
        var templateTypes = (await _templateTypes.GetAllAsync(cancellationToken)).ToList();

        var totalDays = Math.Max(1, (campaign.EndDate.ToDateTime(TimeOnly.MinValue) - campaign.StartDate.ToDateTime(TimeOnly.MinValue)).Days + 1);
        var userPrompt = BuildOptionsPrompt(campaign, goal?.GoalName, promoType?.TypeName, brandProfile, totalDays, templateTypes);
        var provider = _providerFactory.GetProvider(request.Provider);

        var generated = new AiGeneratedContent
        {
            UserId = userId,
            Input = userPrompt,
            Status = (int)AiContentStatus.Processing
        };
        await _unitOfWork.Ai.AddAsync(generated, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            var result = await provider.GenerateAsync(new AiGenerationRequest
            {
                SystemPrompt = TimelineOptionsSystemPrompt,
                UserPrompt = userPrompt,
                MaxTokens = Math.Max(campaign.NumberOfPosts * 3 * 300, 4000) // x3 vì sinh cả 3 phương án cùng lúc
            }, cancellationToken);

            var options = ParseOptions(result.RawContent, templateTypes);
            if (options.Count == 0)
            {
                throw new InvalidOperationException("AI không trả về phương án timeline hợp lệ nào — vui lòng thử lại.");
            }

            generated.Output = result.RawContent;
            generated.TokensUsed = result.TokensUsed;
            generated.Status = (int)AiContentStatus.Completed;
            generated.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Ai.UpdateAsync(generated, cancellationToken);

            await _creditService.DeductCreditsAsync(
                userId, creditCost, AutoWork.Shared.Enums.CreditTransactionType.GenerateContent,
                $"AI timeline options generation: {campaign.Name}", nameof(Campaign), campaign.Id, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new GenerateTimelineOptionsResponse
            {
                CampaignId = campaign.Id,
                Options = options,
                CreditsUsed = creditCost,
                TokensUsed = result.TokensUsed
            };
        }
        catch (Exception ex)
        {
            generated.Status = (int)AiContentStatus.Failed;
            generated.ErrorMessage = ex.Message;
            generated.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Ai.UpdateAsync(generated, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogError(ex, "AI timeline options generation failed for campaign {CampaignId}", campaign.Id);
            throw;
        }
    }

    public async Task<TimelineDto> ConfirmTimelineOptionAsync(
        Guid userId, ConfirmTimelineOptionRequest request, CancellationToken cancellationToken = default)
    {
        var campaign = await _campaigns.GetByIdAsync(request.CampaignId, cancellationToken)
            ?? throw new NotFoundException(nameof(Campaign), request.CampaignId);

        var templateType = await _templateTypes.GetByIdAsync(request.TemplateTypeId, cancellationToken)
            ?? throw new BadRequestException("Mẫu timeline không hợp lệ.");

        var brandProfile = (await _brandProfiles.FindAsync(b => b.ProjectId == campaign.ProjectId, cancellationToken)).FirstOrDefault();

        var timeline = new Timeline
        {
            ProjectId = campaign.ProjectId,
            CampaignId = campaign.Id,
            Name = string.IsNullOrWhiteSpace(request.TimelineName) ? campaign.Name : request.TimelineName,
            TemplateTypeId = request.TemplateTypeId,
            StartDate = campaign.StartDate,
            EndDate = campaign.EndDate,
            Status = (int)TimelineStatus.Active
        };
        await _timelines.AddAsync(timeline, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken); // cần timeline.Id trước khi tạo Post

        foreach (var (draft, index) in request.Posts.Select((d, i) => (d, i)))
        {
            var dayNumber = index + 1;
            var scheduledDate = campaign.StartDate.AddDays(Math.Max(0, draft.DayOffset));
            if (scheduledDate > campaign.EndDate) scheduledDate = campaign.EndDate;

            var post = new Post
            {
                ProjectId = campaign.ProjectId,
                TimelineId = timeline.Id,
                Title = string.IsNullOrWhiteSpace(draft.Title) ? $"Bài viết ngày {dayNumber}" : draft.Title,
                DayNumber = dayNumber,
                ScheduledAt = scheduledDate.ToDateTime(new TimeOnly(9, 0)),
                Status = (int)AutoWork.Domain.Enums.PostStatus.Draft
            };
            await _posts.AddAsync(post, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken); // cần post.Id

            var contentWithFooter = PostContentFooterBuilder.AppendBrandFooter(draft.Content, brandProfile);
            await _postContents.AddAsync(new PostContent
            {
                PostId = post.Id,
                ContentType = 1,
                Content = contentWithFooter,
                SortOrder = 0
            }, cancellationToken);

            foreach (var tag in (draft.Hashtags ?? []).Select(t => t.Trim().TrimStart('#')).Where(t => t.Length > 0).Distinct())
            {
                var existing = (await _hashtags.FindAsync(h => h.Tag == tag, cancellationToken)).FirstOrDefault();
                if (existing is null)
                {
                    existing = new Hashtag { Tag = tag };
                    await _hashtags.AddAsync(existing, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }
                await _postHashtags.AddAsync(new PostHashtag { PostId = post.Id, HashtagId = existing.Id }, cancellationToken);
            }
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        var postCount = request.Posts.Count;
        return new TimelineDto
        {
            Id = timeline.Id,
            ProjectId = timeline.ProjectId,
            CampaignId = timeline.CampaignId,
            Name = timeline.Name,
            TemplateTypeName = templateType.TypeName,
            StartDate = timeline.StartDate,
            EndDate = timeline.EndDate,
            Status = timeline.Status,
            PostCount = postCount,
            CreatedAt = timeline.CreatedAt
        };
    }

    private static string BuildOptionsPrompt(
        Campaign campaign, string? goalName, string? promoTypeName, BrandProfile? brandProfile,
        int totalDays, List<TimelineTemplateType> templateTypes)
    {
        var brandContext = brandProfile is null
            ? "Chưa có thông tin thương hiệu cụ thể — dùng giọng văn thân thiện, chuyên nghiệp mặc định."
            : $"Thương hiệu: {brandProfile.BrandName} (ngành hàng: {brandProfile.Industry ?? "chưa rõ"}). Mô tả: {brandProfile.ShortDescription ?? "N/A"}.";

        var productContext = string.IsNullOrWhiteSpace(campaign.ProductDescription)
            ? "Chưa có mô tả sản phẩm cụ thể — nội dung chung chung theo mục tiêu chiến dịch."
            : $"Mô tả sản phẩm (do AI phân tích ảnh ở bước trước): {campaign.ProductDescription}";

        var templateNames = string.Join(", ", templateTypes.Select(t => t.TypeName));

        return $"""
            Tên chiến dịch: {campaign.Name}
            Mục tiêu: {goalName ?? "N/A"}
            Loại ưu đãi: {promoTypeName ?? "N/A"}{(campaign.DiscountPercent.HasValue ? $" ({campaign.DiscountPercent}%)" : "")}
            Khung thời gian: {campaign.StartDate:yyyy-MM-dd} đến {campaign.EndDate:yyyy-MM-dd} ({totalDays} ngày)
            Số bài viết MỖI phương án: {campaign.NumberOfPosts}
            {productContext}
            {brandContext}
            Các kiểu mẫu cần sinh (đúng theo thứ tự, mỗi kiểu 1 phương án riêng): {templateNames}
            """;
    }

    private List<TimelineOptionDto> ParseOptions(string rawContent, List<TimelineTemplateType> templateTypes)
    {
        try
        {
            var jsonStart = rawContent.IndexOf('{');
            var jsonEnd = rawContent.LastIndexOf('}');
            if (jsonStart < 0 || jsonEnd <= jsonStart) return [];

            var json = rawContent[jsonStart..(jsonEnd + 1)];
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var parsed = JsonSerializer.Deserialize<RawOptionsWrapper>(json, options);
            if (parsed?.Options is null) return [];

            var result = new List<TimelineOptionDto>();
            for (var i = 0; i < parsed.Options.Count; i++)
            {
                var raw = parsed.Options[i];
                // Khớp theo VỊ TRÍ với danh sách TimelineTemplateType thật trong DB (thứ tự đã yêu cầu
                // AI tuân theo trong prompt) — tránh phụ thuộc AI trả về đúng tên chuỗi 100%.
                var templateType = i < templateTypes.Count ? templateTypes[i] : templateTypes.LastOrDefault();
                if (templateType is null) continue;

                result.Add(new TimelineOptionDto
                {
                    TemplateTypeId = templateType.Id,
                    TemplateTypeName = templateType.TypeName,
                    StrategyDescription = raw.StrategyDescription ?? string.Empty,
                    Posts = (raw.Posts ?? []).Select(p => new DraftPostItemDto
                    {
                        DayOffset = p.DayOffset,
                        Title = p.Title ?? string.Empty,
                        Content = p.Content ?? string.Empty,
                        Hashtags = p.Hashtags ?? []
                    }).ToList()
                });
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse AI timeline options JSON response.");
            return [];
        }
    }

    private sealed class RawOptionsWrapper
    {
        public List<RawOption>? Options { get; set; }
    }

    private sealed class RawOption
    {
        public string? TemplateType { get; set; }
        public string? StrategyDescription { get; set; }
        public List<RawPost>? Posts { get; set; }
    }

    private sealed class RawPost
    {
        public int DayOffset { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public List<string>? Hashtags { get; set; }
    }
}
