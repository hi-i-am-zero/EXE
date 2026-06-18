using AutoMapper;
using AutoWork.Application.DTOs.Affiliates;
using AutoWork.Application.DTOs.AI;
using AutoWork.Application.DTOs.Credits;
using AutoWork.Application.DTOs.Dashboard;
using AutoWork.Application.DTOs.Facebook;
using AutoWork.Application.DTOs.Media;
using AutoWork.Application.DTOs.Notifications;
using AutoWork.Application.DTOs.Payments;
using AutoWork.Application.DTOs.Plans;
using AutoWork.Application.DTOs.Posts;
using AutoWork.Application.DTOs.Users;
using AutoWork.Application.DTOs.WordPress;
using AutoWork.Application.DTOs.Zalo;
using AutoWork.Domain.Entities;
using AutoWork.Shared.Enums;

namespace AutoWork.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(d => d.Roles, opt => opt.MapFrom(s => s.UserRoles.Select(ur => ur.Role.Name)));

        CreateMap<CreateUserDto, User>()
            .ForMember(d => d.PasswordHash, opt => opt.Ignore())
            .ForMember(d => d.Id, opt => opt.Ignore());

        CreateMap<UpdateUserDto, User>()
            .ForAllMembers(opts => opts.Condition((_, _, srcMember) => srcMember is not null));

        CreateMap<AiPrompt, AiPromptDto>();

        CreateMap<AiGeneratedContent, AiHistoryDto>()
            .ForMember(d => d.PromptId, opt => opt.MapFrom(s => s.AiPromptId))
            .ForMember(d => d.PromptName, opt => opt.MapFrom(s => s.AiPrompt != null ? s.AiPrompt.Name : null));

        CreateMap<Post, PostDto>()
            .ForMember(d => d.ProjectName, opt => opt.MapFrom(s => s.Project.Name))
            .ForMember(d => d.Schedule, opt => opt.MapFrom(s => s.Schedule));

        CreateMap<CreatePostDto, Post>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.Title, opt => opt.MapFrom(s => s.Title))
            .ForMember(d => d.Status, opt => opt.MapFrom(_ => 1))
            .ForMember(d => d.Contents, opt => opt.Ignore());

        CreateMap<UpdatePostDto, Post>()
            .ForAllMembers(opts => opts.Condition((_, _, srcMember) => srcMember is not null));

        CreateMap<PostSchedule, PostScheduleDto>();

        CreateMap<FacebookAccount, FacebookAccountDto>()
            .ForMember(d => d.Pages, opt => opt.MapFrom(s => s.Pages));

        CreateMap<FacebookPage, FacebookPageDto>();

        CreateMap<WordPressSite, WordPressSiteDto>()
            .ForMember(d => d.IsActive, opt => opt.MapFrom(s => s.IsConnected));

        CreateMap<WordPressPost, WordPressPostDto>()
            .ForMember(d => d.Content, opt => opt.MapFrom(s => s.Excerpt ?? string.Empty))
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.ScheduledAt, opt => opt.MapFrom(s => s.PublishedAt));

        CreateMap<CreateWordPressPostDto, WordPressPost>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.Excerpt, opt => opt.MapFrom(s => s.Content))
            .ForMember(d => d.Status, opt => opt.MapFrom(_ => 1));

        CreateMap<ZaloAccount, ZaloAccountDto>();

        CreateMap<ZaloPost, ZaloPostDto>();

        CreateMap<Plan, PlanDto>();

        CreateMap<Subscription, SubscriptionDto>()
            .ForMember(d => d.PlanName, opt => opt.MapFrom(s => s.Plan.Name));

        CreateMap<Credit, CreditDto>();

        CreateMap<CreditTransaction, CreditTransactionDto>();

        CreateMap<Affiliate, AffiliateDto>()
            .ForMember(d => d.PendingEarnings, opt => opt.MapFrom(_ => 0m));

        CreateMap<AffiliateCommission, AffiliateCommissionDto>()
            .ForMember(d => d.ReferredUserEmail, opt => opt.MapFrom(s => s.ReferredUser.Email));

        CreateMap<Payment, PaymentDto>()
            .ForMember(d => d.Provider, opt => opt.MapFrom(s => (PaymentProvider)s.PaymentMethod))
            .ForMember(d => d.InvoiceId, opt => opt.MapFrom(s => s.InvoiceId))
            .ForMember(d => d.TransactionId, opt => opt.MapFrom(s => s.TransactionId ?? string.Empty))
            .ForMember(d => d.CompletedAt, opt => opt.MapFrom(s => s.PaidAt))
            .ForMember(d => d.Currency, opt => opt.MapFrom(_ => "VND"));

        CreateMap<Invoice, InvoiceDto>()
            .ForMember(d => d.Amount, opt => opt.MapFrom(s => s.Total))
            .ForMember(d => d.Description, opt => opt.MapFrom(s => s.Notes))
            .ForMember(d => d.Currency, opt => opt.MapFrom(_ => "VND"));

        CreateMap<MediaFile, MediaFileDto>()
            .ForMember(d => d.OriginalFileName, opt => opt.MapFrom(s => s.FileName))
            .ForMember(d => d.ContentType, opt => opt.MapFrom(s => s.MimeType))
            .ForMember(d => d.PublicUrl, opt => opt.MapFrom(s => s.FileUrl))
            .ForMember(d => d.FileType, opt => opt.MapFrom(s => ResolveMediaFileType(s.MimeType)));

        CreateMap<Notification, NotificationDto>()
            .ForMember(d => d.Type, opt => opt.MapFrom(s => (NotificationType)s.Type))
            .ForMember(d => d.Link, opt => opt.Ignore());

        CreateMap<AuditLog, RecentActivityDto>()
            .ForMember(d => d.EntityName, opt => opt.MapFrom(s => s.EntityType))
            .ForMember(d => d.Description, opt => opt.MapFrom(s => $"{s.Action} on {s.EntityType}"));
    }

    private static MediaFileType ResolveMediaFileType(string mimeType)
    {
        if (mimeType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            return MediaFileType.Image;
        }

        if (mimeType.StartsWith("video/", StringComparison.OrdinalIgnoreCase))
        {
            return MediaFileType.Video;
        }

        if (mimeType.StartsWith("audio/", StringComparison.OrdinalIgnoreCase))
        {
            return MediaFileType.Audio;
        }

        if (mimeType.Contains("pdf", StringComparison.OrdinalIgnoreCase)
            || mimeType.Contains("document", StringComparison.OrdinalIgnoreCase))
        {
            return MediaFileType.Document;
        }

        return MediaFileType.Other;
    }
}
