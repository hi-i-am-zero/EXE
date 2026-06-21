using AutoWork.Application.Common.Helpers;
using AutoWork.Domain.Entities;
using AutoWork.Domain.Enums;
using AutoWork.Persistence.Context;
using AutoWork.Shared.Constants;
using AutoWork.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AutoWork.Persistence.Seed;

public static partial class DataSeeder
{
    public static readonly Guid SuperAdminUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid SuperAdminRoleId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid AdminRoleId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    public static readonly Guid UserRoleId = Guid.Parse("44444444-4444-4444-4444-444444444444");
    public static readonly Guid FreePlanId = Guid.Parse("55555555-5555-5555-5555-555555555555");
    public static readonly Guid StarterPlanId = Guid.Parse("66666666-6666-6666-6666-666666666666");
    public static readonly Guid ProPlanId = Guid.Parse("77777777-7777-7777-7777-777777777777");
    public static readonly Guid BusinessPlanId = Guid.Parse("88888888-8888-8888-8888-888888888888");

    public static async Task SeedAsync(ApplicationDbContext context, ILogger? logger = null, CancellationToken cancellationToken = default)
    {
        await SeedRolesAsync(context, cancellationToken);
        await SeedPermissionsAsync(context, cancellationToken);
        await SeedRolePermissionsMatrixAsync(context, cancellationToken);
        await SeedPlansAsync(context, cancellationToken);
        await SeedChannelsAsync(context, cancellationToken);
        await SeedSettingsAsync(context, cancellationToken);
        await SeedAiPromptsAsync(context, cancellationToken);
        await SeedExtendedAiPromptsAsync(context, cancellationToken);
        await SeedSuperAdminAsync(context, cancellationToken);
        await SeedDemoUsersAsync(context, cancellationToken);
        await SeedDemoWorkspaceAsync(context, cancellationToken);
        await SeedDemoPostsAsync(context, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
        logger?.LogInformation("Database seed completed successfully.");
    }

    private static async Task SeedRolesAsync(ApplicationDbContext context, CancellationToken cancellationToken)
    {
        if (await context.Roles.AnyAsync(cancellationToken))
        {
            return;
        }

        context.Roles.AddRange(
            new Role
            {
                Id = SuperAdminRoleId,
                Name = AppRoles.SuperAdmin,
                Description = "Full system access",
                IsSystem = true,
                CreatedAt = DateTime.UtcNow
            },
            new Role
            {
                Id = AdminRoleId,
                Name = AppRoles.Admin,
                Description = "Administrative access",
                IsSystem = true,
                CreatedAt = DateTime.UtcNow
            },
            new Role
            {
                Id = UserRoleId,
                Name = AppRoles.User,
                Description = "Standard user access",
                IsSystem = true,
                CreatedAt = DateTime.UtcNow
            });
    }

    private static async Task SeedPermissionsAsync(ApplicationDbContext context, CancellationToken cancellationToken)
    {
        if (await context.Permissions.AnyAsync(cancellationToken))
        {
            return;
        }

        var permissions = AppPermissions.All.Select(code => new Permission
        {
            Id = Guid.NewGuid(),
            Name = code.Split('.').Last(),
            Code = code,
            Module = code.Split('.').First(),
            Description = $"Permission: {code}",
            CreatedAt = DateTime.UtcNow
        });

        context.Permissions.AddRange(permissions.ToList());

        await context.SaveChangesAsync(cancellationToken);

        var allPermissions = await context.Permissions.ToListAsync(cancellationToken);
        var rolePermissions = allPermissions.Select(permission => new RolePermission
        {
            Id = Guid.NewGuid(),
            RoleId = SuperAdminRoleId,
            PermissionId = permission.Id,
            CreatedAt = DateTime.UtcNow
        });
        context.RolePermissions.AddRange(rolePermissions);
    }

    private static async Task SeedPlansAsync(ApplicationDbContext context, CancellationToken cancellationToken)
    {
        if (await context.Plans.AnyAsync(cancellationToken))
        {
            return;
        }

        context.Plans.AddRange(
            new Plan
            {
                Id = FreePlanId,
                Name = "Free",
                Code = nameof(PlanType.Free),
                Description = "Get started with 100 credits per month",
                Price = 0,
                BillingPeriod = (int)BillingPeriod.Monthly,
                MaxProjects = 1,
                MaxChannels = 2,
                MaxPostsPerMonth = 50,
                CreditsIncluded = PlanCredits.Free,
                IsActive = true,
                SortOrder = 1,
                CreatedAt = DateTime.UtcNow
            },
            new Plan
            {
                Id = StarterPlanId,
                Name = "Premium",
                Code = nameof(PlanType.Starter),
                Description = "Gói phổ biến — AI viết & đăng bài đa kênh (Facebook, WordPress, Zalo OA)",
                Price = 299_000,
                BillingPeriod = (int)BillingPeriod.Monthly,
                MaxProjects = 3,
                MaxChannels = 5,
                MaxPostsPerMonth = 200,
                CreditsIncluded = PlanCredits.Starter,
                IsActive = true,
                SortOrder = 2,
                CreatedAt = DateTime.UtcNow
            },
            new Plan
            {
                Id = ProPlanId,
                Name = "Pro",
                Code = nameof(PlanType.Pro),
                Description = "Advanced features for growing businesses",
                Price = 499_000,
                BillingPeriod = (int)BillingPeriod.Monthly,
                MaxProjects = 10,
                MaxChannels = 20,
                MaxPostsPerMonth = 1000,
                CreditsIncluded = PlanCredits.Pro,
                IsActive = true,
                SortOrder = 3,
                CreatedAt = DateTime.UtcNow
            },
            new Plan
            {
                Id = BusinessPlanId,
                Name = "Business",
                Code = nameof(PlanType.Business),
                Description = "Unlimited credits and enterprise support",
                Price = 1_999_000,
                BillingPeriod = (int)BillingPeriod.Monthly,
                MaxProjects = 100,
                MaxChannels = 100,
                MaxPostsPerMonth = 10000,
                CreditsIncluded = int.MaxValue,
                IsActive = true,
                SortOrder = 4,
                CreatedAt = DateTime.UtcNow
            });
    }

    private static async Task SeedChannelsAsync(ApplicationDbContext context, CancellationToken cancellationToken)
    {
        if (await context.Channels.AnyAsync(cancellationToken))
        {
            return;
        }

        context.Channels.AddRange(
            new Channel
            {
                Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                Name = "Facebook",
                Code = "facebook",
                Description = "Publish to Facebook pages",
                IsActive = true,
                SortOrder = 1,
                CreatedAt = DateTime.UtcNow
            },
            new Channel
            {
                Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                Name = "WordPress",
                Code = "wordpress",
                Description = "Publish to WordPress sites",
                IsActive = true,
                SortOrder = 2,
                CreatedAt = DateTime.UtcNow
            },
            new Channel
            {
                Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                Name = "Zalo",
                Code = "zalo",
                Description = "Publish to Zalo Official Account",
                IsActive = true,
                SortOrder = 3,
                CreatedAt = DateTime.UtcNow
            });
    }

    private static async Task SeedSettingsAsync(ApplicationDbContext context, CancellationToken cancellationToken)
    {
        if (await context.Settings.AnyAsync(cancellationToken))
        {
            return;
        }

        var settings = new Dictionary<string, (string Value, string Category, string Description, bool IsPublic)>
        {
            ["App.Name"] = ("AutoWork", "General", "Tên ứng dụng", true),
            ["App.SupportEmail"] = ("support@autowork.com.vn", "General", "Email hỗ trợ", true),
            ["App.SupportPhone"] = ("0394932696", "General", "Hotline hỗ trợ", true),
            ["App.DefaultLanguage"] = ("vi-VN", "General", "Ngôn ngữ mặc định", true),
            ["App.PremiumTrialDays"] = ("1", "General", "Số ngày dùng thử Premium miễn phí", true),
            ["Auth.JwtExpiryMinutes"] = ("60", "Auth", "JWT access token expiry in minutes", false),
            ["Auth.RefreshTokenExpiryDays"] = ("7", "Auth", "Refresh token expiry in days", false),
            ["Credits.DefaultFreeCredits"] = (PlanCredits.Free.ToString(), "Credits", "Credits gói Free", false),
            ["Credits.AiGenerateCost"] = ("5", "Credits", "Chi phí 1 lần tạo AI", false),
            ["Credits.PostPublishCost"] = ("2", "Credits", "Chi phí 1 lần đăng bài", false),
            ["Payment.Currency"] = ("VND", "Payment", "Đơn vị tiền tệ", true),
            ["Affiliate.DefaultCommissionRate"] = ("0.30", "Affiliate", "Hoa hồng affiliate 30%", false),
            ["Affiliate.CookieDays"] = ("30", "Affiliate", "Thời hạn cookie giới thiệu (ngày)", false),
            ["Affiliate.CommissionMonths"] = ("12", "Affiliate", "Thời hạn tính hoa hồng (tháng)", false)
        };

        foreach (var (key, (value, category, description, isPublic)) in settings)
        {
            context.Settings.Add(new Setting
            {
                Id = Guid.NewGuid(),
                Key = key,
                Value = value,
                Category = category,
                Description = description,
                IsPublic = isPublic,
                CreatedAt = DateTime.UtcNow
            });
        }
    }

    private static async Task SeedAiPromptsAsync(ApplicationDbContext context, CancellationToken cancellationToken)
    {
        if (await context.AiPrompts.AnyAsync(cancellationToken))
        {
            return;
        }

        context.AiPrompts.AddRange(
            new AiPrompt
            {
                Id = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                Name = "Blog Post Generator",
                Category = "Content",
                IsSystem = true,
                Template = """
                    Write a blog post about {{topic}}.
                    Keywords: {{keywords}}
                    Tone: {{tone}}
                    Industry: {{industry}}
                    Length: {{length}} words
                    Include a compelling title, meta description, full content, and relevant hashtags.
                    """,
                CreatedAt = DateTime.UtcNow
            },
            new AiPrompt
            {
                Id = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
                Name = "Social Media Post",
                Category = "Social",
                IsSystem = true,
                Template = """
                    Create a social media post for {{topic}}.
                    Platform: {{platform}}
                    Tone: {{tone}}
                    Include hashtags and a call-to-action: {{cta}}
                    """,
                CreatedAt = DateTime.UtcNow
            },
            new AiPrompt
            {
                Id = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                Name = "SEO Article",
                Category = "SEO",
                IsSystem = true,
                Template = """
                    Write an SEO-optimized article about {{topic}}.
                    Focus keyword: {{keywords}}
                    Meta title and meta description required.
                    Target length: {{length}} words.
                    Industry: {{industry}}
                    """,
                CreatedAt = DateTime.UtcNow
            },
            new AiPrompt
            {
                Id = Guid.Parse("10101010-1010-1010-1010-101010101010"),
                Name = "Product Description",
                Category = "E-commerce",
                IsSystem = true,
                Template = """
                    Write a product description for {{topic}}.
                    Include short description and full description.
                    Tone: {{tone}}
                    Highlight key benefits and features.
                    """,
                CreatedAt = DateTime.UtcNow
            });
    }

    private static async Task SeedSuperAdminAsync(ApplicationDbContext context, CancellationToken cancellationToken)
    {
        if (await context.Users.AnyAsync(u => u.Email == "admin@autowork.com", cancellationToken))
        {
            return;
        }

        var superAdmin = new User
        {
            Id = SuperAdminUserId,
            Email = "admin@autowork.com",
            PasswordHash = PasswordHelper.Hash("Admin@123!"),
            FirstName = "Super",
            LastName = "Admin",
            IsActive = true,
            ReferralCode = "SUPERADMIN",
            CreatedAt = DateTime.UtcNow
        };

        context.Users.Add(superAdmin);
        context.UserRoles.Add(new UserRole
        {
            Id = Guid.NewGuid(),
            UserId = SuperAdminUserId,
            RoleId = SuperAdminRoleId,
            CreatedAt = DateTime.UtcNow
        });
        context.Credits.Add(new Credit
        {
            Id = Guid.NewGuid(),
            UserId = SuperAdminUserId,
            Balance = int.MaxValue,
            TotalEarned = int.MaxValue,
            TotalUsed = 0,
            CreatedAt = DateTime.UtcNow
        });
    }
}
