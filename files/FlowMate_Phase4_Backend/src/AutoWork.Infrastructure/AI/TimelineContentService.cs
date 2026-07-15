using System.Text.Json;
using AutoWork.Application.Common.Exceptions;
using AutoWork.Application.DTOs.AI;
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
    private const string TimelineSystemPrompt = """
        You are a professional Vietnamese social media marketing strategist. Given a campaign goal,
        brand voice, and a date range, generate a list of social media post drafts spread across the
        timeline. Respond with ONLY a valid JSON array (no surrounding object, no markdown), each item
        using this schema:
        [
          {
            "dayOffset": 0,
            "title": "string (tiêu đề ngắn)",
            "content": "string (nội dung bài đăng đầy đủ bằng tiếng Việt, có emoji phù hợp)",
            "hashtags": ["tag1", "tag2"]
          }
        ]
        "dayOffset" là số ngày kể từ ngày bắt đầu timeline (0 = ngày đầu tiên). Rải đều các bài viết
        hợp lý trong khoảng thời gian được cung cấp, không trùng lặp ý tưởng giữa các bài.
        """;

    private readonly AiProviderFactory _providerFactory;
    private readonly ICreditService _creditService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<Timeline> _timelines;
    private readonly IRepository<Campaign> _campaigns;
    private readonly IRepository<CampaignGoal> _goals;
    private readonly IRepository<BrandProfile> _brandProfiles;
    private readonly IRepository<Post> _posts;
    private readonly IRepository<PostContent> _postContents;
    private readonly IRepository<Hashtag> _hashtags;
    private readonly IRepository<PostHashtag> _postHashtags;
    private readonly ILogger<TimelineContentService> _logger;

    public TimelineContentService(
        AiProviderFactory providerFactory,
        ICreditService creditService,
        IUnitOfWork unitOfWork,
        IRepository<Timeline> timelines,
        IRepository<Campaign> campaigns,
        IRepository<CampaignGoal> goals,
        IRepository<BrandProfile> brandProfiles,
        IRepository<Post> posts,
        IRepository<PostContent> postContents,
        IRepository<Hashtag> hashtags,
        IRepository<PostHashtag> postHashtags,
        ILogger<TimelineContentService> logger)
    {
        _providerFactory = providerFactory;
        _creditService = creditService;
        _unitOfWork = unitOfWork;
        _timelines = timelines;
        _campaigns = campaigns;
        _goals = goals;
        _brandProfiles = brandProfiles;
        _posts = posts;
        _postContents = postContents;
        _hashtags = hashtags;
        _postHashtags = postHashtags;
        _logger = logger;
    }

    public async Task<GenerateTimelinePostsResponse> GenerateTimelinePostsAsync(
        Guid userId,
        GenerateTimelinePostsRequest request,
        CancellationToken cancellationToken = default)
    {
        var creditCost = CreditCosts.GenerateContent;
        if (!await _creditService.HasSufficientCreditsAsync(userId, creditCost, cancellationToken))
        {
            throw new BadRequestException("Insufficient credits for AI timeline generation.");
        }

        var timeline = await _timelines.GetByIdAsync(request.TimelineId, cancellationToken)
            ?? throw new NotFoundException(nameof(Timeline), request.TimelineId);

        var campaign = timeline.CampaignId.HasValue
            ? await _campaigns.GetByIdAsync(timeline.CampaignId.Value, cancellationToken)
            : null;
        var goalName = campaign is not null
            ? (await _goals.GetByIdAsync(campaign.GoalId, cancellationToken))?.GoalName
            : null;

        var brandProfile = (await _brandProfiles.FindAsync(b => b.ProjectId == timeline.ProjectId, cancellationToken))
            .FirstOrDefault();

        var totalDays = Math.Max(1, (timeline.EndDate.ToDateTime(TimeOnly.MinValue) - timeline.StartDate.ToDateTime(TimeOnly.MinValue)).Days + 1);
        var postCount = request.PostCount ?? campaign?.NumberOfPosts ?? Math.Min(5, totalDays);
        postCount = Math.Clamp(postCount, 1, 30); // chặn AI tạo quá nhiều bài trong 1 lần gọi (chi phí + token limit)

        var userPrompt = BuildUserPrompt(timeline, goalName, brandProfile, totalDays, postCount);
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
                SystemPrompt = TimelineSystemPrompt,
                UserPrompt = userPrompt,
                MaxTokens = Math.Max(postCount * 350, 1500)
            }, cancellationToken);

            var drafts = ParsePostDraftArray(result.RawContent);
            if (drafts.Count == 0)
            {
                throw new InvalidOperationException("AI không trả về bài viết hợp lệ nào — vui lòng thử lại.");
            }

            var createdPosts = await CreateDraftPostsAsync(timeline, drafts, cancellationToken);

            generated.Output = result.RawContent;
            generated.TokensUsed = result.TokensUsed;
            generated.Status = (int)AiContentStatus.Completed;
            generated.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Ai.UpdateAsync(generated, cancellationToken);

            await _creditService.DeductCreditsAsync(
                userId,
                creditCost,
                CreditTransactionType.GenerateContent,
                $"AI timeline generation: {timeline.Name}",
                nameof(Timeline),
                timeline.Id,
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new GenerateTimelinePostsResponse
            {
                TimelineId = timeline.Id,
                Posts = createdPosts,
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

            _logger.LogError(ex, "AI timeline generation failed for timeline {TimelineId}", timeline.Id);
            throw;
        }
    }

    private static string BuildUserPrompt(Timeline timeline, string? goalName, BrandProfile? brandProfile, int totalDays, int postCount)
    {
        var brandContext = brandProfile is null
            ? "Chưa có thông tin thương hiệu cụ thể — dùng giọng văn thân thiện, chuyên nghiệp mặc định."
            : $"Thương hiệu: {brandProfile.BrandName} (ngành hàng: {brandProfile.Industry ?? "chưa rõ"}). " +
              $"Mô tả: {brandProfile.ShortDescription ?? "N/A"}.";

        return $"""
            Tên timeline: {timeline.Name}
            Mục tiêu chiến dịch: {goalName ?? "Không gắn chiến dịch cụ thể — nội dung chung chung theo thương hiệu"}
            Khung thời gian: {timeline.StartDate:yyyy-MM-dd} đến {timeline.EndDate:yyyy-MM-dd} ({totalDays} ngày)
            Số bài viết cần tạo: {postCount}
            {brandContext}
            """;
    }

    private List<TimelinePostDraft> ParsePostDraftArray(string rawContent)
    {
        try
        {
            var jsonStart = rawContent.IndexOf('[');
            var jsonEnd = rawContent.LastIndexOf(']');
            if (jsonStart < 0 || jsonEnd <= jsonStart)
            {
                return [];
            }

            var json = rawContent[jsonStart..(jsonEnd + 1)];
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<List<TimelinePostDraft>>(json, options) ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse AI timeline JSON array response.");
            return [];
        }
    }

    private async Task<List<GeneratedPostDraftDto>> CreateDraftPostsAsync(
        Timeline timeline, List<TimelinePostDraft> drafts, CancellationToken cancellationToken)
    {
        var result = new List<GeneratedPostDraftDto>();

        foreach (var (draft, index) in drafts.Select((d, i) => (d, i)))
        {
            var dayNumber = index + 1;
            var scheduledDate = timeline.StartDate.AddDays(Math.Max(0, draft.DayOffset));
            if (scheduledDate > timeline.EndDate)
            {
                scheduledDate = timeline.EndDate;
            }

            var post = new Post
            {
                ProjectId = timeline.ProjectId,
                TimelineId = timeline.Id,
                Title = string.IsNullOrWhiteSpace(draft.Title) ? $"Bài viết ngày {dayNumber}" : draft.Title,
                DayNumber = dayNumber,
                ScheduledAt = scheduledDate.ToDateTime(new TimeOnly(9, 0)),
                Status = 0 // Draft — người dùng xem lại/chỉnh sửa trước khi thật sự lên lịch đăng
            };
            await _posts.AddAsync(post, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken); // cần Post.Id trước khi tạo PostContent/Hashtag

            await _postContents.AddAsync(new PostContent
            {
                PostId = post.Id,
                ContentType = 1, // Text
                Content = draft.Content,
                SortOrder = 0
            }, cancellationToken);

            var hashtagIds = new List<Guid>();
            foreach (var tag in (draft.Hashtags ?? []).Select(t => t.Trim().TrimStart('#')).Where(t => t.Length > 0).Distinct())
            {
                var existing = (await _hashtags.FindAsync(h => h.Tag == tag, cancellationToken)).FirstOrDefault();
                if (existing is null)
                {
                    existing = new Hashtag { Tag = tag };
                    await _hashtags.AddAsync(existing, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }
                hashtagIds.Add(existing.Id);
            }

            foreach (var hashtagId in hashtagIds)
            {
                await _postHashtags.AddAsync(new PostHashtag { PostId = post.Id, HashtagId = hashtagId }, cancellationToken);
            }
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            result.Add(new GeneratedPostDraftDto
            {
                PostId = post.Id,
                DayNumber = dayNumber,
                ScheduledAt = post.ScheduledAt,
                Title = post.Title,
                Content = draft.Content,
                Hashtags = draft.Hashtags ?? []
            });
        }

        return result;
    }

    private sealed class TimelinePostDraft
    {
        public int DayOffset { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public List<string>? Hashtags { get; set; }
    }
}
