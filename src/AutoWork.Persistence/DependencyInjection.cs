using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Persistence.Context;
using AutoWork.Persistence.Repositories;
using AutoWork.Persistence.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AutoWork.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                sqlOptions.EnableRetryOnFailure(3);
            }));

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPostRepository, PostRepository>();
        services.AddScoped<IPlanRepository, PlanRepository>();
        services.AddScoped<ICreditRepository, CreditRepository>();
        services.AddScoped<IAiRepository, AiRepository>();
        services.AddScoped<IFacebookRepository, FacebookRepository>();
        services.AddScoped<IWordPressRepository, WordPressRepository>();
        services.AddScoped<IZaloRepository, ZaloRepository>();
        services.AddScoped<IScheduleRepository, ScheduleRepository>();
        services.AddScoped<IAffiliateRepository, AffiliateRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IMediaRepository, MediaRepository>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IBrandProfileRepository, BrandProfileRepository>();
        services.AddScoped<ICampaignRepository, CampaignRepository>();
        services.AddScoped<ITimelineRepository, TimelineRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }

    public static async Task InitializeDatabaseAsync(
        this IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetService<ILogger<ApplicationDbContext>>();

        var hasExistingSchema = await TableExistsAsync(context, "Users", cancellationToken);
        var hasMigrationHistory = await TableExistsAsync(context, "__EFMigrationsHistory", cancellationToken);

        if (hasExistingSchema && !hasMigrationHistory)
        {
            logger?.LogInformation("Detected FlowMate/SQL schema without EF history — applying compat layer and skipping MigrateAsync.");
            await ApplyFlowMateCompatAsync(context, logger, cancellationToken);
        }
        else
        {
            await context.Database.MigrateAsync(cancellationToken);
        }

        await DataSeeder.SeedAsync(context, logger, cancellationToken);
    }

    private static async Task<bool> TableExistsAsync(
        ApplicationDbContext context,
        string tableName,
        CancellationToken cancellationToken)
    {
        await context.Database.OpenConnectionAsync(cancellationToken);
        try
        {
            await using var command = context.Database.GetDbConnection().CreateCommand();
            command.CommandText = """
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM INFORMATION_SCHEMA.TABLES
                    WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = @table
                ) THEN 1 ELSE 0 END
                """;
            var parameter = command.CreateParameter();
            parameter.ParameterName = "@table";
            parameter.Value = tableName;
            command.Parameters.Add(parameter);
            var result = await command.ExecuteScalarAsync(cancellationToken);
            return Convert.ToInt32(result) == 1;
        }
        finally
        {
            await context.Database.CloseConnectionAsync();
        }
    }

    private static async Task ApplyFlowMateCompatAsync(
        ApplicationDbContext context,
        ILogger? logger,
        CancellationToken cancellationToken)
    {
        var statements = new[]
        {
            """
            IF COL_LENGTH('dbo.Posts', 'ChannelAccountId') IS NULL
                ALTER TABLE dbo.Posts ADD ChannelAccountId UNIQUEIDENTIFIER NULL;
            """,
            """
            IF COL_LENGTH('dbo.Posts', 'ExternalPostId') IS NULL
                ALTER TABLE dbo.Posts ADD ExternalPostId NVARCHAR(128) NULL;
            """,
            """
            IF COL_LENGTH('dbo.Posts', 'PublishedUrl') IS NULL
                ALTER TABLE dbo.Posts ADD PublishedUrl NVARCHAR(1000) NULL;
            """,
            """
            IF COL_LENGTH('dbo.Posts', 'PublishedAt') IS NULL
                ALTER TABLE dbo.Posts ADD PublishedAt DATETIME2 NULL;
            """,
            """
            IF COL_LENGTH('dbo.PostSchedules', 'PostId') IS NULL
                ALTER TABLE dbo.PostSchedules ADD PostId UNIQUEIDENTIFIER NULL;
            """,
            """
            IF COL_LENGTH('dbo.PostSchedules', 'PostChannelAccountId') IS NOT NULL
               AND COLUMNPROPERTY(OBJECT_ID('dbo.PostSchedules'), 'PostChannelAccountId', 'AllowsNull') = 0
            BEGIN
                ALTER TABLE dbo.PostSchedules ALTER COLUMN PostChannelAccountId UNIQUEIDENTIFIER NULL;
            END
            """,
            // ===== FlowMate v2: các bảng mới (Brand Memory / Campaign / Timeline / đăng đa kênh) =====
            // Mỗi statement idempotent (IF OBJECT_ID(...) IS NULL) để chạy lại nhiều lần không lỗi.
            """
            IF COL_LENGTH('dbo.ChannelAccounts', 'ParentChannelAccountId') IS NULL
                ALTER TABLE dbo.ChannelAccounts ADD ParentChannelAccountId UNIQUEIDENTIFIER NULL;
            """,
            """
            IF COL_LENGTH('dbo.ChannelAccounts', 'AccessToken') IS NULL
                ALTER TABLE dbo.ChannelAccounts ADD AccessToken NVARCHAR(MAX) NULL;
            """,
            """
            IF COL_LENGTH('dbo.ChannelAccounts', 'RefreshToken') IS NULL
                ALTER TABLE dbo.ChannelAccounts ADD RefreshToken NVARCHAR(MAX) NULL;
            """,
            """
            IF COL_LENGTH('dbo.ChannelAccounts', 'TokenExpiresAt') IS NULL
                ALTER TABLE dbo.ChannelAccounts ADD TokenExpiresAt DATETIME2 NULL;
            """,
            """
            IF COL_LENGTH('dbo.ChannelAccounts', 'Scope') IS NULL
                ALTER TABLE dbo.ChannelAccounts ADD Scope NVARCHAR(500) NULL;
            """,
            """
            IF COL_LENGTH('dbo.Posts', 'VoiceSampleId') IS NULL
                ALTER TABLE dbo.Posts ADD VoiceSampleId UNIQUEIDENTIFIER NULL;
            """,
            """
            IF COL_LENGTH('dbo.Posts', 'DayNumber') IS NULL
                ALTER TABLE dbo.Posts ADD DayNumber INT NULL;
            """,
            """
            IF COL_LENGTH('dbo.Posts', 'TrackingEnabled') IS NULL
                ALTER TABLE dbo.Posts ADD TrackingEnabled BIT NOT NULL DEFAULT CAST(1 AS BIT);
            """,
            """
            IF COL_LENGTH('dbo.MediaFiles', 'ProductId') IS NULL
                ALTER TABLE dbo.MediaFiles ADD ProductId UNIQUEIDENTIFIER NULL;
            """,
            """
            IF COL_LENGTH('dbo.MediaFiles', 'StoragePath') IS NULL
                ALTER TABLE dbo.MediaFiles ADD StoragePath NVARCHAR(1000) NOT NULL DEFAULT '';
            """,
            """
            IF COL_LENGTH('dbo.FacebookAccounts', 'ProjectId') IS NULL
                ALTER TABLE dbo.FacebookAccounts ADD ProjectId UNIQUEIDENTIFIER NOT NULL
                    DEFAULT '00000000-0000-0000-0000-000000000000';
            """,
            """
            IF COL_LENGTH('dbo.Campaigns', 'ProductDescription') IS NULL
                ALTER TABLE dbo.Campaigns ADD ProductDescription NVARCHAR(MAX) NULL;
            """,
            """
            IF OBJECT_ID('dbo.ProjectMembers') IS NULL
            CREATE TABLE dbo.ProjectMembers (
                Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, ProjectId UNIQUEIDENTIFIER NOT NULL,
                UserId UNIQUEIDENTIFIER NOT NULL, Role NVARCHAR(20) NOT NULL DEFAULT 'Member',
                InvitedAt DATETIME2 NULL, JoinedAt DATETIME2 NULL,
                CreatedAt DATETIME2 NOT NULL, UpdatedAt DATETIME2 NULL,
                CreatedBy UNIQUEIDENTIFIER NULL, UpdatedBy UNIQUEIDENTIFIER NULL, IsDeleted BIT NOT NULL DEFAULT 0,
                CONSTRAINT FK_ProjectMembers_Projects FOREIGN KEY (ProjectId) REFERENCES dbo.Projects(Id),
                CONSTRAINT FK_ProjectMembers_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(Id));
            """,
            """
            IF OBJECT_ID('dbo.ProjectMembers') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ProjectMembers_ProjectId_UserId')
                CREATE UNIQUE INDEX IX_ProjectMembers_ProjectId_UserId ON dbo.ProjectMembers(ProjectId, UserId);
            """,
            """
            IF OBJECT_ID('dbo.VoiceSampleTemplates') IS NULL
            CREATE TABLE dbo.VoiceSampleTemplates (
                Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, SampleText NVARCHAR(500) NOT NULL,
                CreatedAt DATETIME2 NOT NULL, UpdatedAt DATETIME2 NULL,
                CreatedBy UNIQUEIDENTIFIER NULL, UpdatedBy UNIQUEIDENTIFIER NULL, IsDeleted BIT NOT NULL DEFAULT 0);
            """,
            """
            IF OBJECT_ID('dbo.BrandStyles') IS NULL
            CREATE TABLE dbo.BrandStyles (
                Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, StyleName NVARCHAR(50) NOT NULL,
                CreatedAt DATETIME2 NOT NULL, UpdatedAt DATETIME2 NULL,
                CreatedBy UNIQUEIDENTIFIER NULL, UpdatedBy UNIQUEIDENTIFIER NULL, IsDeleted BIT NOT NULL DEFAULT 0);
            """,
            """
            IF OBJECT_ID('dbo.BrandStyles') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_BrandStyles_StyleName')
                CREATE UNIQUE INDEX IX_BrandStyles_StyleName ON dbo.BrandStyles(StyleName);
            """,
            """
            IF OBJECT_ID('dbo.ToneKeywords') IS NULL
            CREATE TABLE dbo.ToneKeywords (
                Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, Keyword NVARCHAR(50) NOT NULL,
                CreatedAt DATETIME2 NOT NULL, UpdatedAt DATETIME2 NULL,
                CreatedBy UNIQUEIDENTIFIER NULL, UpdatedBy UNIQUEIDENTIFIER NULL, IsDeleted BIT NOT NULL DEFAULT 0);
            """,
            """
            IF OBJECT_ID('dbo.ToneKeywords') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ToneKeywords_Keyword')
                CREATE UNIQUE INDEX IX_ToneKeywords_Keyword ON dbo.ToneKeywords(Keyword);
            """,
            """
            IF OBJECT_ID('dbo.CtaTemplates') IS NULL
            CREATE TABLE dbo.CtaTemplates (
                Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, CtaText NVARCHAR(100) NOT NULL,
                CreatedAt DATETIME2 NOT NULL, UpdatedAt DATETIME2 NULL,
                CreatedBy UNIQUEIDENTIFIER NULL, UpdatedBy UNIQUEIDENTIFIER NULL, IsDeleted BIT NOT NULL DEFAULT 0);
            """,
            """
            IF OBJECT_ID('dbo.CtaTemplates') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_CtaTemplates_CtaText')
                CREATE UNIQUE INDEX IX_CtaTemplates_CtaText ON dbo.CtaTemplates(CtaText);
            """,
            """
            IF OBJECT_ID('dbo.Hashtags') IS NULL
            CREATE TABLE dbo.Hashtags (
                Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, Tag NVARCHAR(50) NOT NULL,
                CreatedAt DATETIME2 NOT NULL, UpdatedAt DATETIME2 NULL,
                CreatedBy UNIQUEIDENTIFIER NULL, UpdatedBy UNIQUEIDENTIFIER NULL, IsDeleted BIT NOT NULL DEFAULT 0);
            """,
            """
            IF OBJECT_ID('dbo.Hashtags') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Hashtags_Tag')
                CREATE UNIQUE INDEX IX_Hashtags_Tag ON dbo.Hashtags(Tag);
            """,
            """
            IF OBJECT_ID('dbo.BrandProfiles') IS NULL
            CREATE TABLE dbo.BrandProfiles (
                Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, ProjectId UNIQUEIDENTIFIER NOT NULL,
                LogoUrl NVARCHAR(500) NULL, BusinessName NVARCHAR(150) NOT NULL, BrandName NVARCHAR(150) NOT NULL,
                Industry NVARCHAR(100) NULL, FoundingYear SMALLINT NULL, ShortDescription NVARCHAR(1000) NULL,
                Address NVARCHAR(300) NULL, ContactPhone NVARCHAR(20) NULL, ContactEmail NVARCHAR(255) NULL,
                Website NVARCHAR(255) NULL, VoiceSampleId UNIQUEIDENTIFIER NULL,
                CreatedAt DATETIME2 NOT NULL, UpdatedAt DATETIME2 NULL,
                CreatedBy UNIQUEIDENTIFIER NULL, UpdatedBy UNIQUEIDENTIFIER NULL, IsDeleted BIT NOT NULL DEFAULT 0,
                CONSTRAINT FK_BrandProfiles_Projects FOREIGN KEY (ProjectId) REFERENCES dbo.Projects(Id),
                CONSTRAINT FK_BrandProfiles_VoiceSampleTemplates FOREIGN KEY (VoiceSampleId) REFERENCES dbo.VoiceSampleTemplates(Id));
            """,
            """
            IF OBJECT_ID('dbo.BrandProfiles') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_BrandProfiles_ProjectId')
                CREATE UNIQUE INDEX IX_BrandProfiles_ProjectId ON dbo.BrandProfiles(ProjectId);
            """,
            """
            IF OBJECT_ID('dbo.BrandProfileStyles') IS NULL
            CREATE TABLE dbo.BrandProfileStyles (
                Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, BrandProfileId UNIQUEIDENTIFIER NOT NULL, StyleId UNIQUEIDENTIFIER NOT NULL,
                CreatedAt DATETIME2 NOT NULL, UpdatedAt DATETIME2 NULL,
                CreatedBy UNIQUEIDENTIFIER NULL, UpdatedBy UNIQUEIDENTIFIER NULL, IsDeleted BIT NOT NULL DEFAULT 0,
                CONSTRAINT FK_BrandProfileStyles_BrandProfiles FOREIGN KEY (BrandProfileId) REFERENCES dbo.BrandProfiles(Id),
                CONSTRAINT FK_BrandProfileStyles_BrandStyles FOREIGN KEY (StyleId) REFERENCES dbo.BrandStyles(Id));
            """,
            """
            IF OBJECT_ID('dbo.BrandProfileStyles') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_BrandProfileStyles_BrandProfileId_StyleId')
                CREATE UNIQUE INDEX IX_BrandProfileStyles_BrandProfileId_StyleId ON dbo.BrandProfileStyles(BrandProfileId, StyleId);
            """,
            """
            IF OBJECT_ID('dbo.BrandProfileKeywords') IS NULL
            CREATE TABLE dbo.BrandProfileKeywords (
                Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, BrandProfileId UNIQUEIDENTIFIER NOT NULL, ToneKeywordId UNIQUEIDENTIFIER NOT NULL,
                CreatedAt DATETIME2 NOT NULL, UpdatedAt DATETIME2 NULL,
                CreatedBy UNIQUEIDENTIFIER NULL, UpdatedBy UNIQUEIDENTIFIER NULL, IsDeleted BIT NOT NULL DEFAULT 0,
                CONSTRAINT FK_BrandProfileKeywords_BrandProfiles FOREIGN KEY (BrandProfileId) REFERENCES dbo.BrandProfiles(Id),
                CONSTRAINT FK_BrandProfileKeywords_ToneKeywords FOREIGN KEY (ToneKeywordId) REFERENCES dbo.ToneKeywords(Id));
            """,
            """
            IF OBJECT_ID('dbo.BrandProfileKeywords') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_BrandProfileKeywords_BrandProfileId_ToneKeywordId')
                CREATE UNIQUE INDEX IX_BrandProfileKeywords_BrandProfileId_ToneKeywordId ON dbo.BrandProfileKeywords(BrandProfileId, ToneKeywordId);
            """,
            """
            IF OBJECT_ID('dbo.BrandCtas') IS NULL
            CREATE TABLE dbo.BrandCtas (
                Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, BrandProfileId UNIQUEIDENTIFIER NOT NULL, CtaId UNIQUEIDENTIFIER NOT NULL,
                CreatedAt DATETIME2 NOT NULL, UpdatedAt DATETIME2 NULL,
                CreatedBy UNIQUEIDENTIFIER NULL, UpdatedBy UNIQUEIDENTIFIER NULL, IsDeleted BIT NOT NULL DEFAULT 0,
                CONSTRAINT FK_BrandCtas_BrandProfiles FOREIGN KEY (BrandProfileId) REFERENCES dbo.BrandProfiles(Id),
                CONSTRAINT FK_BrandCtas_CtaTemplates FOREIGN KEY (CtaId) REFERENCES dbo.CtaTemplates(Id));
            """,
            """
            IF OBJECT_ID('dbo.BrandCtas') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_BrandCtas_BrandProfileId_CtaId')
                CREATE UNIQUE INDEX IX_BrandCtas_BrandProfileId_CtaId ON dbo.BrandCtas(BrandProfileId, CtaId);
            """,
            """
            IF OBJECT_ID('dbo.BrandHashtags') IS NULL
            CREATE TABLE dbo.BrandHashtags (
                Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, BrandProfileId UNIQUEIDENTIFIER NOT NULL, HashtagId UNIQUEIDENTIFIER NOT NULL,
                CreatedAt DATETIME2 NOT NULL, UpdatedAt DATETIME2 NULL,
                CreatedBy UNIQUEIDENTIFIER NULL, UpdatedBy UNIQUEIDENTIFIER NULL, IsDeleted BIT NOT NULL DEFAULT 0,
                CONSTRAINT FK_BrandHashtags_BrandProfiles FOREIGN KEY (BrandProfileId) REFERENCES dbo.BrandProfiles(Id),
                CONSTRAINT FK_BrandHashtags_Hashtags FOREIGN KEY (HashtagId) REFERENCES dbo.Hashtags(Id));
            """,
            """
            IF OBJECT_ID('dbo.BrandHashtags') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_BrandHashtags_BrandProfileId_HashtagId')
                CREATE UNIQUE INDEX IX_BrandHashtags_BrandProfileId_HashtagId ON dbo.BrandHashtags(BrandProfileId, HashtagId);
            """,
            """
            IF OBJECT_ID('dbo.BrandColors') IS NULL
            CREATE TABLE dbo.BrandColors (
                Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, BrandProfileId UNIQUEIDENTIFIER NOT NULL,
                ColorHex NVARCHAR(9) NOT NULL, ColorType NVARCHAR(20) NOT NULL DEFAULT 'Secondary', SortOrder INT NOT NULL DEFAULT 0,
                CreatedAt DATETIME2 NOT NULL, UpdatedAt DATETIME2 NULL,
                CreatedBy UNIQUEIDENTIFIER NULL, UpdatedBy UNIQUEIDENTIFIER NULL, IsDeleted BIT NOT NULL DEFAULT 0,
                CONSTRAINT FK_BrandColors_BrandProfiles FOREIGN KEY (BrandProfileId) REFERENCES dbo.BrandProfiles(Id));
            """,
            """
            IF OBJECT_ID('dbo.BrandFonts') IS NULL
            CREATE TABLE dbo.BrandFonts (
                Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, BrandProfileId UNIQUEIDENTIFIER NOT NULL,
                FontName NVARCHAR(100) NOT NULL, UsageType NVARCHAR(20) NOT NULL,
                CreatedAt DATETIME2 NOT NULL, UpdatedAt DATETIME2 NULL,
                CreatedBy UNIQUEIDENTIFIER NULL, UpdatedBy UNIQUEIDENTIFIER NULL, IsDeleted BIT NOT NULL DEFAULT 0,
                CONSTRAINT FK_BrandFonts_BrandProfiles FOREIGN KEY (BrandProfileId) REFERENCES dbo.BrandProfiles(Id));
            """,
            """
            IF OBJECT_ID('dbo.ProductCategories') IS NULL
            CREATE TABLE dbo.ProductCategories (
                Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, BrandProfileId UNIQUEIDENTIFIER NOT NULL, CategoryName NVARCHAR(100) NOT NULL,
                CreatedAt DATETIME2 NOT NULL, UpdatedAt DATETIME2 NULL,
                CreatedBy UNIQUEIDENTIFIER NULL, UpdatedBy UNIQUEIDENTIFIER NULL, IsDeleted BIT NOT NULL DEFAULT 0,
                CONSTRAINT FK_ProductCategories_BrandProfiles FOREIGN KEY (BrandProfileId) REFERENCES dbo.BrandProfiles(Id));
            """,
            """
            IF OBJECT_ID('dbo.ProductCategories') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ProductCategories_BrandProfileId_CategoryName')
                CREATE UNIQUE INDEX IX_ProductCategories_BrandProfileId_CategoryName ON dbo.ProductCategories(BrandProfileId, CategoryName);
            """,
            """
            IF OBJECT_ID('dbo.Products') IS NULL
            CREATE TABLE dbo.Products (
                Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, BrandProfileId UNIQUEIDENTIFIER NOT NULL, ProductCategoryId UNIQUEIDENTIFIER NULL,
                ProductName NVARCHAR(200) NOT NULL, Price DECIMAL(18,2) NULL, AiGeneratedDescription NVARCHAR(MAX) NULL,
                CreatedAt DATETIME2 NOT NULL, UpdatedAt DATETIME2 NULL,
                CreatedBy UNIQUEIDENTIFIER NULL, UpdatedBy UNIQUEIDENTIFIER NULL, IsDeleted BIT NOT NULL DEFAULT 0,
                CONSTRAINT FK_Products_BrandProfiles FOREIGN KEY (BrandProfileId) REFERENCES dbo.BrandProfiles(Id),
                CONSTRAINT FK_Products_ProductCategories FOREIGN KEY (ProductCategoryId) REFERENCES dbo.ProductCategories(Id));
            """,
            """
            IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_MediaFiles_Products_ProductId') AND OBJECT_ID('dbo.Products') IS NOT NULL
                ALTER TABLE dbo.MediaFiles ADD CONSTRAINT FK_MediaFiles_Products_ProductId FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id);
            """,
            """
            IF OBJECT_ID('dbo.CampaignGoals') IS NULL
            CREATE TABLE dbo.CampaignGoals (
                Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, GoalName NVARCHAR(100) NOT NULL,
                CreatedAt DATETIME2 NOT NULL, UpdatedAt DATETIME2 NULL,
                CreatedBy UNIQUEIDENTIFIER NULL, UpdatedBy UNIQUEIDENTIFIER NULL, IsDeleted BIT NOT NULL DEFAULT 0);
            """,
            """
            IF OBJECT_ID('dbo.CampaignGoals') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_CampaignGoals_GoalName')
                CREATE UNIQUE INDEX IX_CampaignGoals_GoalName ON dbo.CampaignGoals(GoalName);
            """,
            """
            IF OBJECT_ID('dbo.PromotionTypes') IS NULL
            CREATE TABLE dbo.PromotionTypes (
                Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, TypeName NVARCHAR(100) NOT NULL,
                CreatedAt DATETIME2 NOT NULL, UpdatedAt DATETIME2 NULL,
                CreatedBy UNIQUEIDENTIFIER NULL, UpdatedBy UNIQUEIDENTIFIER NULL, IsDeleted BIT NOT NULL DEFAULT 0);
            """,
            """
            IF OBJECT_ID('dbo.PromotionTypes') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PromotionTypes_TypeName')
                CREATE UNIQUE INDEX IX_PromotionTypes_TypeName ON dbo.PromotionTypes(TypeName);
            """,
            """
            IF OBJECT_ID('dbo.TimelineTemplateTypes') IS NULL
            CREATE TABLE dbo.TimelineTemplateTypes (
                Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, TypeName NVARCHAR(50) NOT NULL, Description NVARCHAR(300) NULL,
                CreatedAt DATETIME2 NOT NULL, UpdatedAt DATETIME2 NULL,
                CreatedBy UNIQUEIDENTIFIER NULL, UpdatedBy UNIQUEIDENTIFIER NULL, IsDeleted BIT NOT NULL DEFAULT 0);
            """,
            """
            IF OBJECT_ID('dbo.TimelineTemplateTypes') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TimelineTemplateTypes_TypeName')
                CREATE UNIQUE INDEX IX_TimelineTemplateTypes_TypeName ON dbo.TimelineTemplateTypes(TypeName);
            """,
            """
            IF OBJECT_ID('dbo.Campaigns') IS NULL
            CREATE TABLE dbo.Campaigns (
                Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, ProjectId UNIQUEIDENTIFIER NOT NULL, Name NVARCHAR(200) NOT NULL,
                GoalId UNIQUEIDENTIFIER NOT NULL, PromotionTypeId UNIQUEIDENTIFIER NOT NULL,
                DiscountPercent DECIMAL(5,2) NULL, DiscountAmount DECIMAL(18,2) NULL, MinOrderAmount DECIMAL(18,2) NULL,
                StartDate DATE NOT NULL, EndDate DATE NOT NULL, NumberOfPosts INT NOT NULL DEFAULT 1, Status INT NOT NULL,
                CreatedAt DATETIME2 NOT NULL, UpdatedAt DATETIME2 NULL,
                CreatedBy UNIQUEIDENTIFIER NULL, UpdatedBy UNIQUEIDENTIFIER NULL, IsDeleted BIT NOT NULL DEFAULT 0,
                CONSTRAINT FK_Campaigns_Projects FOREIGN KEY (ProjectId) REFERENCES dbo.Projects(Id),
                CONSTRAINT FK_Campaigns_CampaignGoals FOREIGN KEY (GoalId) REFERENCES dbo.CampaignGoals(Id),
                CONSTRAINT FK_Campaigns_PromotionTypes FOREIGN KEY (PromotionTypeId) REFERENCES dbo.PromotionTypes(Id),
                CONSTRAINT CK_Campaigns_Dates CHECK (EndDate >= StartDate),
                CONSTRAINT CK_Campaigns_NumberOfPosts CHECK (NumberOfPosts > 0));
            """,
            """
            IF OBJECT_ID('dbo.CampaignChannelAccounts') IS NULL
            CREATE TABLE dbo.CampaignChannelAccounts (
                Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, CampaignId UNIQUEIDENTIFIER NOT NULL, ChannelAccountId UNIQUEIDENTIFIER NOT NULL,
                CreatedAt DATETIME2 NOT NULL, UpdatedAt DATETIME2 NULL,
                CreatedBy UNIQUEIDENTIFIER NULL, UpdatedBy UNIQUEIDENTIFIER NULL, IsDeleted BIT NOT NULL DEFAULT 0,
                CONSTRAINT FK_CampaignChannelAccounts_Campaigns FOREIGN KEY (CampaignId) REFERENCES dbo.Campaigns(Id),
                CONSTRAINT FK_CampaignChannelAccounts_ChannelAccounts FOREIGN KEY (ChannelAccountId) REFERENCES dbo.ChannelAccounts(Id));
            """,
            """
            IF OBJECT_ID('dbo.CampaignChannelAccounts') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_CampaignChannelAccounts_CampaignId_ChannelAccountId')
                CREATE UNIQUE INDEX IX_CampaignChannelAccounts_CampaignId_ChannelAccountId ON dbo.CampaignChannelAccounts(CampaignId, ChannelAccountId);
            """,
            """
            IF OBJECT_ID('dbo.Timelines') IS NULL
            CREATE TABLE dbo.Timelines (
                Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, ProjectId UNIQUEIDENTIFIER NOT NULL, CampaignId UNIQUEIDENTIFIER NULL,
                Name NVARCHAR(200) NOT NULL, TemplateTypeId UNIQUEIDENTIFIER NOT NULL,
                StartDate DATE NOT NULL, EndDate DATE NOT NULL, Status INT NOT NULL,
                CreatedAt DATETIME2 NOT NULL, UpdatedAt DATETIME2 NULL,
                CreatedBy UNIQUEIDENTIFIER NULL, UpdatedBy UNIQUEIDENTIFIER NULL, IsDeleted BIT NOT NULL DEFAULT 0,
                CONSTRAINT FK_Timelines_Projects FOREIGN KEY (ProjectId) REFERENCES dbo.Projects(Id),
                CONSTRAINT FK_Timelines_Campaigns FOREIGN KEY (CampaignId) REFERENCES dbo.Campaigns(Id),
                CONSTRAINT FK_Timelines_TimelineTemplateTypes FOREIGN KEY (TemplateTypeId) REFERENCES dbo.TimelineTemplateTypes(Id),
                CONSTRAINT CK_Timelines_Dates CHECK (EndDate >= StartDate));
            """,
            """
            IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Posts_Timelines_TimelineId') AND OBJECT_ID('dbo.Timelines') IS NOT NULL
                ALTER TABLE dbo.Posts ADD CONSTRAINT FK_Posts_Timelines_TimelineId FOREIGN KEY (TimelineId) REFERENCES dbo.Timelines(Id);
            """,
            """
            IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Posts_VoiceSampleTemplates_VoiceSampleId') AND OBJECT_ID('dbo.VoiceSampleTemplates') IS NOT NULL
                ALTER TABLE dbo.Posts ADD CONSTRAINT FK_Posts_VoiceSampleTemplates_VoiceSampleId FOREIGN KEY (VoiceSampleId) REFERENCES dbo.VoiceSampleTemplates(Id);
            """,
            """
            IF OBJECT_ID('dbo.PostHashtags') IS NULL
            CREATE TABLE dbo.PostHashtags (
                Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, PostId UNIQUEIDENTIFIER NOT NULL, HashtagId UNIQUEIDENTIFIER NOT NULL,
                CreatedAt DATETIME2 NOT NULL, UpdatedAt DATETIME2 NULL,
                CreatedBy UNIQUEIDENTIFIER NULL, UpdatedBy UNIQUEIDENTIFIER NULL, IsDeleted BIT NOT NULL DEFAULT 0,
                CONSTRAINT FK_PostHashtags_Posts FOREIGN KEY (PostId) REFERENCES dbo.Posts(Id),
                CONSTRAINT FK_PostHashtags_Hashtags FOREIGN KEY (HashtagId) REFERENCES dbo.Hashtags(Id));
            """,
            """
            IF OBJECT_ID('dbo.PostChannelAccounts') IS NULL
            CREATE TABLE dbo.PostChannelAccounts (
                Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, PostId UNIQUEIDENTIFIER NOT NULL, ChannelAccountId UNIQUEIDENTIFIER NOT NULL,
                Status INT NOT NULL, ExternalPostId NVARCHAR(128) NULL, PublishedUrl NVARCHAR(1000) NULL, PublishedAt DATETIME2 NULL,
                CreatedAt DATETIME2 NOT NULL, UpdatedAt DATETIME2 NULL,
                CreatedBy UNIQUEIDENTIFIER NULL, UpdatedBy UNIQUEIDENTIFIER NULL, IsDeleted BIT NOT NULL DEFAULT 0,
                CONSTRAINT FK_PostChannelAccounts_Posts FOREIGN KEY (PostId) REFERENCES dbo.Posts(Id),
                CONSTRAINT FK_PostChannelAccounts_ChannelAccounts FOREIGN KEY (ChannelAccountId) REFERENCES dbo.ChannelAccounts(Id));
            """,
            """
            IF OBJECT_ID('dbo.PostChannelAccounts') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PostChannelAccounts_PostId_ChannelAccountId')
                CREATE UNIQUE INDEX IX_PostChannelAccounts_PostId_ChannelAccountId ON dbo.PostChannelAccounts(PostId, ChannelAccountId);
            """,
            """
            IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_PostSchedules_PostChannelAccounts_PostChannelAccountId') AND OBJECT_ID('dbo.PostChannelAccounts') IS NOT NULL
                ALTER TABLE dbo.PostSchedules ADD CONSTRAINT FK_PostSchedules_PostChannelAccounts_PostChannelAccountId FOREIGN KEY (PostChannelAccountId) REFERENCES dbo.PostChannelAccounts(Id);
            """,
            """
            IF OBJECT_ID('dbo.PostAnalytics') IS NULL
            CREATE TABLE dbo.PostAnalytics (
                Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, PostChannelAccountId UNIQUEIDENTIFIER NOT NULL,
                Reach INT NOT NULL DEFAULT 0, Likes INT NOT NULL DEFAULT 0, Comments INT NOT NULL DEFAULT 0, Shares INT NOT NULL DEFAULT 0,
                RecordedAt DATETIME2 NOT NULL,
                CreatedAt DATETIME2 NOT NULL, UpdatedAt DATETIME2 NULL,
                CreatedBy UNIQUEIDENTIFIER NULL, UpdatedBy UNIQUEIDENTIFIER NULL, IsDeleted BIT NOT NULL DEFAULT 0,
                CONSTRAINT FK_PostAnalytics_PostChannelAccounts FOREIGN KEY (PostChannelAccountId) REFERENCES dbo.PostChannelAccounts(Id));
            """,
            """
            IF OBJECT_ID('dbo.PostAnalytics') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PostAnalytics_PostChannelAccountId')
                CREATE UNIQUE INDEX IX_PostAnalytics_PostChannelAccountId ON dbo.PostAnalytics(PostChannelAccountId);
            """
        };

        foreach (var sql in statements)
        {
            await context.Database.ExecuteSqlRawAsync(sql, cancellationToken);
        }

        logger?.LogInformation("FlowMate EF compatibility layer applied.");
    }
}
