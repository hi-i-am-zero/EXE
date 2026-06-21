using AutoWork.Application.Common.Helpers;
using AutoWork.Domain.Entities;
using AutoWork.Domain.Enums;
using DomainPostStatus = AutoWork.Domain.Enums.PostStatus;
using DomainSubscriptionStatus = AutoWork.Domain.Enums.SubscriptionStatus;
using AutoWork.Persistence.Context;
using AutoWork.Shared.Constants;
using AutoWork.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace AutoWork.Persistence.Seed;

public static partial class DataSeeder
{
    public static readonly Guid DemoUserId = Guid.Parse("12121212-1212-1212-1212-121212121212");
    public static readonly Guid DemoProjectId = Guid.Parse("13131313-1313-1313-1313-131313131313");
    public static readonly Guid DemoChannelAccountFbId = Guid.Parse("14141414-1414-1414-1414-141414141414");
    public static readonly Guid FacebookChannelId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    public static readonly Guid WordPressChannelId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    public static readonly Guid ZaloChannelId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

    private static async Task SeedRolePermissionsMatrixAsync(ApplicationDbContext context, CancellationToken cancellationToken)
    {
        if (await context.RolePermissions.AnyAsync(rp => rp.RoleId == AdminRoleId, cancellationToken))
        {
            return;
        }

        var permissions = await context.Permissions.ToListAsync(cancellationToken);
        var adminCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            AppPermissions.Users.View, AppPermissions.Users.Update,
            AppPermissions.Plans.View, AppPermissions.Subscriptions.View, AppPermissions.Subscriptions.Manage,
            AppPermissions.Posts.View, AppPermissions.Posts.Create, AppPermissions.Posts.Update,
            AppPermissions.Posts.Delete, AppPermissions.Posts.Publish, AppPermissions.Posts.Schedule,
            AppPermissions.Content.Generate, AppPermissions.Content.Approve,
            AppPermissions.Credits.View, AppPermissions.Credits.Manage,
            AppPermissions.Payments.View, AppPermissions.Affiliates.View, AppPermissions.Affiliates.Manage,
            AppPermissions.Media.Upload, AppPermissions.Media.Manage,
            AppPermissions.Settings.View, AppPermissions.Reports.View,
            AppPermissions.Notifications.View, AppPermissions.AuditLogs.View
        };

        var userCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            AppPermissions.Posts.View, AppPermissions.Posts.Create, AppPermissions.Posts.Update,
            AppPermissions.Posts.Publish, AppPermissions.Posts.Schedule,
            AppPermissions.Content.Generate,
            AppPermissions.Credits.View,
            AppPermissions.Plans.View, AppPermissions.Subscriptions.View,
            AppPermissions.Media.Upload,
            AppPermissions.Notifications.View, AppPermissions.Affiliates.View
        };

        foreach (var permission in permissions)
        {
            if (adminCodes.Contains(permission.Code))
            {
                context.RolePermissions.Add(new RolePermission
                {
                    Id = Guid.NewGuid(),
                    RoleId = AdminRoleId,
                    PermissionId = permission.Id,
                    CreatedAt = DateTime.UtcNow
                });
            }

            if (userCodes.Contains(permission.Code))
            {
                context.RolePermissions.Add(new RolePermission
                {
                    Id = Guid.NewGuid(),
                    RoleId = UserRoleId,
                    PermissionId = permission.Id,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }
    }

    private static async Task SeedDemoUsersAsync(ApplicationDbContext context, CancellationToken cancellationToken)
    {
        if (await context.Users.AnyAsync(u => u.Email == "demo@autowork.com", cancellationToken))
        {
            return;
        }

        var demoUser = new User
        {
            Id = DemoUserId,
            Email = "demo@autowork.com",
            PasswordHash = PasswordHelper.Hash("Demo@123!"),
            FirstName = "Demo",
            LastName = "Marketer",
            Phone = "0394932696",
            IsActive = true,
            ReferralCode = "DEMO2026",
            CreatedAt = DateTime.UtcNow
        };

        context.Users.Add(demoUser);
        context.UserRoles.Add(new UserRole
        {
            Id = Guid.NewGuid(),
            UserId = DemoUserId,
            RoleId = UserRoleId,
            CreatedAt = DateTime.UtcNow
        });
        context.Credits.Add(new Credit
        {
            Id = Guid.NewGuid(),
            UserId = DemoUserId,
            Balance = PlanCredits.Starter,
            TotalEarned = PlanCredits.Starter,
            TotalUsed = 0,
            CreatedAt = DateTime.UtcNow
        });
        context.Subscriptions.Add(new Subscription
        {
            Id = Guid.NewGuid(),
            UserId = DemoUserId,
            PlanId = StarterPlanId,
            Status = (int)DomainSubscriptionStatus.Active,
            StartDate = DateTime.UtcNow.AddDays(-15),
            EndDate = DateTime.UtcNow.AddMonths(1),
            AutoRenew = true,
            CreatedAt = DateTime.UtcNow
        });
        context.Affiliates.Add(new Affiliate
        {
            Id = Guid.NewGuid(),
            UserId = DemoUserId,
            Code = "DEMO2026",
            CommissionRate = 0.30m,
            TotalEarnings = 0,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });
    }

    private static async Task SeedDemoWorkspaceAsync(ApplicationDbContext context, CancellationToken cancellationToken)
    {
        if (await context.Projects.AnyAsync(p => p.Id == DemoProjectId, cancellationToken))
        {
            return;
        }

        context.Projects.Add(new Project
        {
            Id = DemoProjectId,
            UserId = DemoUserId,
            Name = "Shop Thời Trang Hương",
            Description = "Dự án marketing đa kênh: Facebook Fanpage, WordPress blog, Zalo OA",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });

        context.ChannelAccounts.AddRange(
            new ChannelAccount
            {
                Id = DemoChannelAccountFbId,
                ProjectId = DemoProjectId,
                ChannelId = FacebookChannelId,
                UserId = DemoUserId,
                Name = "Fanpage Shop Hương",
                ExternalId = "fb_page_demo_001",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new ChannelAccount
            {
                Id = Guid.Parse("15151515-1515-1515-1515-151515151515"),
                ProjectId = DemoProjectId,
                ChannelId = WordPressChannelId,
                UserId = DemoUserId,
                Name = "Blog WordPress",
                ExternalId = "wp_site_demo_001",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new ChannelAccount
            {
                Id = Guid.Parse("16161616-1616-1616-1616-161616161616"),
                ProjectId = DemoProjectId,
                ChannelId = ZaloChannelId,
                UserId = DemoUserId,
                Name = "Zalo OA Shop Hương",
                ExternalId = "zalo_oa_demo_001",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
    }

    private static async Task SeedDemoPostsAsync(ApplicationDbContext context, CancellationToken cancellationToken)
    {
        if (await context.Posts.AnyAsync(p => p.ProjectId == DemoProjectId, cancellationToken))
        {
            return;
        }

        var now = DateTime.UtcNow;
        var posts = new[]
        {
            ("Bộ sương mù mới — Giảm 20% tuần này", DomainPostStatus.Published, now.AddDays(-2)),
            ("5 tips mix đồ công sở thanh lịch", DomainPostStatus.Scheduled, now.AddDays(1)),
            ("Bài SEO: Xu hướng thời trang 2026", DomainPostStatus.Draft, now)
        };

        foreach (var (title, status, created) in posts)
        {
            var postId = Guid.NewGuid();
            context.Posts.Add(new Post
            {
                Id = postId,
                ProjectId = DemoProjectId,
                ChannelAccountId = DemoChannelAccountFbId,
                Title = title,
                Status = (int)status,
                PublishedAt = status == DomainPostStatus.Published ? created : null,
                CreatedAt = created
            });
            context.PostContents.Add(new PostContent
            {
                Id = Guid.NewGuid(),
                PostId = postId,
                ContentType = (int)PostContentType.Text,
                Content = $"Nội dung mẫu cho: {title}",
                SortOrder = 0,
                CreatedAt = created
            });

            if (status == DomainPostStatus.Scheduled)
            {
                context.PostSchedules.Add(new PostSchedule
                {
                    Id = Guid.NewGuid(),
                    PostId = postId,
                    ScheduledAt = now.AddDays(1).AddHours(19),
                    Status = (int)PostScheduleStatus.Pending,
                    CreatedAt = created
                });
            }
        }
    }

    private static async Task SeedExtendedAiPromptsAsync(ApplicationDbContext context, CancellationToken cancellationToken)
    {
        if (await context.AiPrompts.AnyAsync(p => p.Name == "Facebook Fanpage Bán Hàng", cancellationToken))
        {
            return;
        }

        context.AiPrompts.AddRange(
            new AiPrompt
            {
                Id = Guid.Parse("20202020-2020-2020-2020-202020202020"),
                Name = "Facebook Fanpage Bán Hàng",
                Category = "Facebook",
                IsSystem = true,
                Template = """
                    Viết bài đăng Facebook Fanpage bán {{topic}}.
                    Giọng điệu: {{tone}}
                    CTA: {{cta}}
                    Thêm emoji phù hợp, hashtag và lời kêu gọi mua hàng.
                    """,
                CreatedAt = DateTime.UtcNow
            },
            new AiPrompt
            {
                Id = Guid.Parse("21212121-2121-2121-2121-212121212121"),
                Name = "WordPress SEO + WooCommerce",
                Category = "WordPress",
                IsSystem = true,
                Template = """
                    Viết bài chuẩn SEO WordPress về {{topic}}.
                    Focus keyword: {{keywords}}
                    Meta title, meta description, tags.
                    Chèn link sản phẩm WooCommerce nếu có.
                    Độ dài: {{length}} từ.
                    """,
                CreatedAt = DateTime.UtcNow
            },
            new AiPrompt
            {
                Id = Guid.Parse("24242424-2424-2424-2424-242424242424"),
                Name = "Zalo OA Chăm Sóc Khách",
                Category = "Zalo",
                IsSystem = true,
                Template = """
                    Viết tin Zalo OA cho {{topic}}.
                    Giọng điệu thân thiện, ngắn gọn.
                    CTA: {{cta}}
                    """,
                CreatedAt = DateTime.UtcNow
            },
            new AiPrompt
            {
                Id = Guid.Parse("23232323-2323-2323-2323-232323232323"),
                Name = "Auto Comment Fanpage",
                Category = "Facebook",
                IsSystem = true,
                Template = """
                    Trả lời bình luận khách hàng trên Fanpage.
                    Bình luận gốc: {{input}}
                    Giọng điệu: {{tone}}
                    Nhận diện lead nóng và gợi ý chuyển inbox nếu cần.
                    """,
                CreatedAt = DateTime.UtcNow
            });
    }
}
