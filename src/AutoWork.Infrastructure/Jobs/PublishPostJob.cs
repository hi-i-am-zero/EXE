using AutoWork.Application.DTOs.Facebook;
using AutoWork.Application.DTOs.WordPress;
using AutoWork.Application.DTOs.Zalo;
using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Application.Interfaces.Services;
using AutoWork.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace AutoWork.Infrastructure.Jobs;

public class PublishPostJob
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFacebookService _facebookService;
    private readonly IWordPressService _wordPressService;
    private readonly IZaloService _zaloService;
    private readonly ILogger<PublishPostJob> _logger;

    public PublishPostJob(IUnitOfWork unitOfWork, IFacebookService facebookService, IWordPressService wordPressService, IZaloService zaloService, ILogger<PublishPostJob> logger)
    {
        _unitOfWork = unitOfWork; _facebookService = facebookService; _wordPressService = wordPressService; _zaloService = zaloService; _logger = logger;
    }

    public async Task ExecuteAsync(Guid postId, CancellationToken cancellationToken = default)
    {
        var post = await _unitOfWork.Posts.GetByIdWithDetailsAsync(postId, cancellationToken);
        if (post?.Schedule is null) return;
        var schedule = post.Schedule;
        try
        {
            var content = post.Contents.OrderBy(c => c.SortOrder).FirstOrDefault()?.Content ?? post.Title;

            // FlowMate v2: nếu bài viết chọn nhiều nền tảng (PostChannelAccounts), đăng lên TỪNG nền
            // tảng và cập nhật trạng thái riêng cho từng cái. Nếu là bài viết cũ chỉ có 1 kênh đơn lẻ
            // (post.ChannelAccount, trước khi có tính năng đa nền tảng), giữ nguyên luồng cũ làm fallback.
            if (post.PostChannelAccounts.Count > 0)
            {
                var anySuccess = false;
                var anyFailure = false;

                foreach (var target in post.PostChannelAccounts)
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(target.ChannelAccount.ExternalId))
                        {
                            throw new InvalidOperationException("Channel not linked.");
                        }

                        var externalId = await PublishAsync(
                            target.ChannelAccount.UserId,
                            target.ChannelAccount.Channel?.Code ?? string.Empty,
                            Guid.Parse(target.ChannelAccount.ExternalId!),
                            post.Title, content, cancellationToken);

                        target.Status = (int)PostStatus.Published;
                        target.ExternalPostId = externalId;
                        target.PublishedAt = DateTime.UtcNow;
                        anySuccess = true;
                    }
                    catch (Exception ex)
                    {
                        target.Status = (int)PostStatus.Failed;
                        anyFailure = true;
                        _logger.LogError(ex, "Publish failed for post {PostId} on channel account {ChannelAccountId}", postId, target.ChannelAccountId);
                    }
                }

                post.Status = anySuccess ? (int)PostStatus.Published : (int)PostStatus.Failed;
                post.PublishedAt = anySuccess ? DateTime.UtcNow : post.PublishedAt;
                post.UpdatedAt = DateTime.UtcNow;

                schedule.Status = anyFailure && !anySuccess
                    ? (schedule.RetryCount >= 3 ? (int)PostScheduleStatus.Failed : (int)PostScheduleStatus.Pending)
                    : (int)PostScheduleStatus.Completed;
                if (anyFailure) schedule.RetryCount++;
                schedule.ExecutedAt = DateTime.UtcNow;
                schedule.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.Posts.UpdateAsync(post, cancellationToken);
                await _unitOfWork.Schedules.UpdateAsync(schedule, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return;
            }

            if (string.IsNullOrWhiteSpace(post.ChannelAccount?.ExternalId))
                throw new InvalidOperationException("Channel not linked.");
            var channelAccount = post.ChannelAccount!;
            var legacyExternalId = await PublishAsync(channelAccount.UserId, channelAccount.Channel?.Code ?? string.Empty, Guid.Parse(channelAccount.ExternalId!), post.Title, content, cancellationToken);
            post.ExternalPostId = legacyExternalId; post.PublishedAt = DateTime.UtcNow; post.Status = (int)PostStatus.Published; post.UpdatedAt = DateTime.UtcNow;
            schedule.Status = (int)PostScheduleStatus.Completed; schedule.ExecutedAt = DateTime.UtcNow; schedule.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Posts.UpdateAsync(post, cancellationToken);
            await _unitOfWork.Schedules.UpdateAsync(schedule, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            schedule.RetryCount++; schedule.FailureReason = ex.Message;
            schedule.Status = schedule.RetryCount >= 3 ? (int)PostScheduleStatus.Failed : (int)PostScheduleStatus.Pending;
            schedule.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Schedules.UpdateAsync(schedule, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogError(ex, "Publish failed for post {PostId}", postId);
            throw;
        }
    }

    private Task<string> PublishAsync(Guid userId, string channelCode, Guid linkedId, string title, string content, CancellationToken ct) =>
        channelCode.ToLowerInvariant() switch
        {
            "facebook" => _facebookService.PublishPostAsync(userId, new PublishFacebookPostDto { FacebookPageId = linkedId, Message = $"{title}\n\n{content}" }, ct),
            "wordpress" => _wordPressService.PublishPostAsync(userId, new CreateWordPressPostDto { WordPressSiteId = linkedId, Title = title, Content = content, Status = "publish" }, ct),
            "zalo" => _zaloService.PublishPostAsync(userId, new ZaloPostDto { ZaloAccountId = linkedId, Title = title, Content = content }, ct),
            _ => throw new NotSupportedException($"Channel '{channelCode}' unsupported.")
        };
}
