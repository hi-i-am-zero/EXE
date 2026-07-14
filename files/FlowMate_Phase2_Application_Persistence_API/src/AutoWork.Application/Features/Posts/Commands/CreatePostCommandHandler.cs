using AutoWork.Application.Common.Exceptions;
using AutoWork.Application.DTOs.Posts;
using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Application.Interfaces.Services;
using AutoWork.Domain.Entities;
using MediatR;

namespace AutoWork.Application.Features.Posts.Commands;

public class CreatePostCommandHandler : IRequestHandler<CreatePostCommand, PostDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<ChannelAccount> _channelAccounts;
    private readonly IRepository<PostChannelAccount> _postChannelAccounts;
    private readonly IRepository<Hashtag> _hashtags;
    private readonly IRepository<PostHashtag> _postHashtags;
    private readonly ICurrentUserService _currentUserService;

    public CreatePostCommandHandler(
        IUnitOfWork unitOfWork,
        IRepository<ChannelAccount> channelAccounts,
        IRepository<PostChannelAccount> postChannelAccounts,
        IRepository<Hashtag> hashtags,
        IRepository<PostHashtag> postHashtags,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _channelAccounts = channelAccounts;
        _postChannelAccounts = postChannelAccounts;
        _hashtags = hashtags;
        _postHashtags = postHashtags;
        _currentUserService = currentUserService;
    }

    public async Task<PostDto> Handle(CreatePostCommand command, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is not Guid userId)
        {
            throw new UnauthorizedException("User is not authenticated.");
        }

        var project = await _unitOfWork.Projects.GetByIdAsync(command.Request.ProjectId, cancellationToken);
        if (project is null || project.UserId != userId)
        {
            throw new NotFoundException(nameof(Project), command.Request.ProjectId);
        }

        // FlowMate v2: gộp ChannelAccountId đơn lẻ (legacy) + ChannelAccountIds đa chọn thành 1 danh sách duy nhất
        var targetChannelIds = command.Request.ChannelAccountIds.ToList();
        if (command.Request.ChannelAccountId != Guid.Empty && !targetChannelIds.Contains(command.Request.ChannelAccountId))
        {
            targetChannelIds.Add(command.Request.ChannelAccountId);
        }

        var validChannels = await _channelAccounts.FindAsync(
            ca => ca.ProjectId == command.Request.ProjectId && targetChannelIds.Contains(ca.Id), cancellationToken);

        if (validChannels.Count == 0)
        {
            throw new BadRequestException("Không tìm thấy nền tảng hợp lệ để đăng bài.");
        }

        var post = new Post
        {
            ProjectId = command.Request.ProjectId,
            ChannelAccountId = validChannels[0].Id, // giữ 1 giá trị "chính" để tương thích màn hình/API cũ
            TimelineId = command.Request.TimelineId,
            VoiceSampleId = command.Request.VoiceSampleId,
            Title = command.Request.Title.Trim(),
            Status = 1,
            Contents =
            [
                new PostContent
                {
                    ContentType = command.Request.ContentType,
                    Content = command.Request.Content,
                    SortOrder = 0,
                    MediaFileId = command.Request.MediaFileId
                }
            ]
        };

        await _unitOfWork.Posts.AddAsync(post, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Tạo 1 dòng PostChannelAccount cho MỖI nền tảng được chọn — đây là bảng N-N chính
        // cho phép 1 bài viết đăng lên nhiều nền tảng, mỗi nền tảng có trạng thái riêng.
        foreach (var channel in validChannels)
        {
            await _postChannelAccounts.AddAsync(
                new PostChannelAccount { PostId = post.Id, ChannelAccountId = channel.Id, Status = post.Status },
                cancellationToken);
        }

        // Hashtag: tìm-hoặc-tạo trong bảng Hashtags dùng chung (không tạo trùng theo tag text).
        // Tối ưu: gom TẤT CẢ hashtag mới cần tạo vào 1 lần SaveChanges duy nhất (thay vì save
        // từng cái trong vòng lặp) để giảm số round-trip tới DB khi bài viết có nhiều hashtag mới.
        var normalizedTags = command.Request.Hashtags
            .Select(t => t.Trim().TrimStart('#'))
            .Where(t => t.Length > 0)
            .Distinct()
            .ToList();

        if (normalizedTags.Count > 0)
        {
            var existingTags = await _hashtags.FindAsync(h => normalizedTags.Contains(h.Tag), cancellationToken);
            var existingByTag = existingTags.ToDictionary(h => h.Tag);

            var newTags = normalizedTags.Where(t => !existingByTag.ContainsKey(t)).ToList();
            var createdTags = new List<Hashtag>();
            foreach (var tag in newTags)
            {
                var hashtag = new Hashtag { Tag = tag };
                await _hashtags.AddAsync(hashtag, cancellationToken);
                createdTags.Add(hashtag);
            }

            if (createdTags.Count > 0)
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken); // 1 lần duy nhất để sinh Id cho tag mới
            }

            var allTagIds = existingTags.Select(h => h.Id).Concat(createdTags.Select(h => h.Id));
            foreach (var hashtagId in allTagIds)
            {
                await _postHashtags.AddAsync(new PostHashtag { PostId = post.Id, HashtagId = hashtagId }, cancellationToken);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var created = await _unitOfWork.Posts.GetByIdWithDetailsAsync(post.Id, cancellationToken) ?? post;
        return MapToDto(created);
    }

    private static PostDto MapToDto(Post post) => new()
    {
        Id = post.Id,
        ProjectId = post.ProjectId,
        ChannelAccountId = post.ChannelAccountId ?? Guid.Empty,
        TimelineId = post.TimelineId,
        Title = post.Title,
        Status = post.Status,
        ExternalPostId = post.ExternalPostId,
        PublishedUrl = post.PublishedUrl,
        PublishedAt = post.PublishedAt,
        CreatedAt = post.CreatedAt,
        Hashtags = post.PostHashtags.Select(ph => ph.Hashtag.Tag).ToList(),
        Channels = post.PostChannelAccounts.Select(pca => new PostChannelAccountDto
        {
            Id = pca.Id,
            ChannelAccountId = pca.ChannelAccountId,
            ChannelName = pca.ChannelAccount.Channel.Name,
            ChannelAccountName = pca.ChannelAccount.Name,
            Status = pca.Status,
            ExternalPostId = pca.ExternalPostId,
            PublishedUrl = pca.PublishedUrl,
            PublishedAt = pca.PublishedAt
        }).ToList()
    };
}
