/* ============================================================================
   FLOWMATE v2 — INCREMENTAL MIGRATION (chỉ thêm mới, không đụng dữ liệu cũ)
   Dùng khi KHÔNG chạy được `dotnet ef migrations add` (thiếu SDK/mạng).
   Cách dùng đúng chuẩn (khuyến nghị) khi có máy dev đầy đủ dotnet SDK:
       dotnet ef migrations add FlowMateV2_BrandCampaignTimeline ^
           --project src/AutoWork.Persistence --startup-project src/AutoWork.API
       dotnet ef database update ^
           --project src/AutoWork.Persistence --startup-project src/AutoWork.API
   Script này là fallback/tham chiếu để soát lại migration EF Core generate ra
   có khớp ý đồ thiết kế không.
   ============================================================================ */

USE [YourDatabaseName]; -- đổi lại đúng tên DB AutoWork/FlowMate đang dùng
GO

/* ---------- 1) Cột mới trên 3 bảng đã tồn tại ---------- */

IF COL_LENGTH('MediaFiles', 'ProductId') IS NULL
    ALTER TABLE [MediaFiles] ADD [ProductId] uniqueidentifier NULL;
GO
IF COL_LENGTH('MediaFiles', 'StoragePath') IS NULL
    ALTER TABLE [MediaFiles] ADD [StoragePath] nvarchar(1000) NOT NULL DEFAULT '';
GO
IF COL_LENGTH('FacebookAccounts', 'ProjectId') IS NULL
    ALTER TABLE [FacebookAccounts] ADD [ProjectId] uniqueidentifier NOT NULL
        DEFAULT '00000000-0000-0000-0000-000000000000';
GO
IF COL_LENGTH('ChannelAccounts', 'ParentChannelAccountId') IS NULL
    ALTER TABLE [ChannelAccounts] ADD [ParentChannelAccountId] uniqueidentifier NULL;
GO
IF COL_LENGTH('ChannelAccounts', 'AccessToken') IS NULL
    ALTER TABLE [ChannelAccounts] ADD [AccessToken] nvarchar(max) NULL;
GO
IF COL_LENGTH('ChannelAccounts', 'RefreshToken') IS NULL
    ALTER TABLE [ChannelAccounts] ADD [RefreshToken] nvarchar(max) NULL;
GO
IF COL_LENGTH('ChannelAccounts', 'TokenExpiresAt') IS NULL
    ALTER TABLE [ChannelAccounts] ADD [TokenExpiresAt] datetime2 NULL;
GO
IF COL_LENGTH('ChannelAccounts', 'Scope') IS NULL
    ALTER TABLE [ChannelAccounts] ADD [Scope] nvarchar(500) NULL;
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_ChannelAccounts_ChannelAccounts_ParentChannelAccountId')
    ALTER TABLE [ChannelAccounts] ADD CONSTRAINT [FK_ChannelAccounts_ChannelAccounts_ParentChannelAccountId]
        FOREIGN KEY ([ParentChannelAccountId]) REFERENCES [ChannelAccounts]([Id]);
GO

IF COL_LENGTH('Posts', 'VoiceSampleId') IS NULL
    ALTER TABLE [Posts] ADD [VoiceSampleId] uniqueidentifier NULL;
GO
IF COL_LENGTH('Posts', 'DayNumber') IS NULL
    ALTER TABLE [Posts] ADD [DayNumber] int NULL;
GO
IF COL_LENGTH('Posts', 'TrackingEnabled') IS NULL
    ALTER TABLE [Posts] ADD [TrackingEnabled] bit NOT NULL DEFAULT CAST(1 AS bit);
GO
-- Posts.TimelineId / ScheduledAt / ChannelAccountId nullable: đã có sẵn theo comment trong Post.cs,
-- chỉ thêm nếu DB thực tế chưa có (phòng trường hợp EF chưa migrate lần trước):
IF COL_LENGTH('Posts', 'TimelineId') IS NULL
    ALTER TABLE [Posts] ADD [TimelineId] uniqueidentifier NULL;
GO

IF COL_LENGTH('PostSchedules', 'PostChannelAccountId') IS NULL
    ALTER TABLE [PostSchedules] ADD [PostChannelAccountId] uniqueidentifier NULL;
GO

/* ---------- 2) Bảng workspace member (mới) ---------- */
IF OBJECT_ID('ProjectMembers') IS NULL
CREATE TABLE [ProjectMembers] (
    [Id] uniqueidentifier NOT NULL,
    [ProjectId] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [Role] nvarchar(20) NOT NULL DEFAULT 'Member',
    [InvitedAt] datetime2 NULL,
    [JoinedAt] datetime2 NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedBy] uniqueidentifier NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    CONSTRAINT [PK_ProjectMembers] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ProjectMembers_Projects_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [Projects]([Id]),
    CONSTRAINT [FK_ProjectMembers_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users]([Id])
);
GO
IF OBJECT_ID('ProjectMembers') IS NOT NULL AND NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ProjectMembers_ProjectId_UserId')
    CREATE UNIQUE INDEX [IX_ProjectMembers_ProjectId_UserId] ON [ProjectMembers]([ProjectId],[UserId]);
GO

/* ---------- 3) Brand Memory ---------- */
IF OBJECT_ID('VoiceSampleTemplates') IS NULL
CREATE TABLE [VoiceSampleTemplates] (
    [Id] uniqueidentifier NOT NULL,
    [SampleText] nvarchar(500) NOT NULL,
    [CreatedAt] datetime2 NOT NULL, [UpdatedAt] datetime2 NULL,
    [CreatedBy] uniqueidentifier NULL, [UpdatedBy] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    CONSTRAINT [PK_VoiceSampleTemplates] PRIMARY KEY ([Id])
);
GO

IF OBJECT_ID('BrandStyles') IS NULL
CREATE TABLE [BrandStyles] (
    [Id] uniqueidentifier NOT NULL,
    [StyleName] nvarchar(50) NOT NULL,
    [CreatedAt] datetime2 NOT NULL, [UpdatedAt] datetime2 NULL,
    [CreatedBy] uniqueidentifier NULL, [UpdatedBy] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    CONSTRAINT [PK_BrandStyles] PRIMARY KEY ([Id])
);
GO
IF OBJECT_ID('BrandStyles') IS NOT NULL AND NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_BrandStyles_StyleName')
    CREATE UNIQUE INDEX [IX_BrandStyles_StyleName] ON [BrandStyles]([StyleName]);
GO

IF OBJECT_ID('ToneKeywords') IS NULL
CREATE TABLE [ToneKeywords] (
    [Id] uniqueidentifier NOT NULL,
    [Keyword] nvarchar(50) NOT NULL,
    [CreatedAt] datetime2 NOT NULL, [UpdatedAt] datetime2 NULL,
    [CreatedBy] uniqueidentifier NULL, [UpdatedBy] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    CONSTRAINT [PK_ToneKeywords] PRIMARY KEY ([Id])
);
GO
IF OBJECT_ID('ToneKeywords') IS NOT NULL AND NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ToneKeywords_Keyword')
    CREATE UNIQUE INDEX [IX_ToneKeywords_Keyword] ON [ToneKeywords]([Keyword]);
GO

IF OBJECT_ID('CtaTemplates') IS NULL
CREATE TABLE [CtaTemplates] (
    [Id] uniqueidentifier NOT NULL,
    [CtaText] nvarchar(100) NOT NULL,
    [CreatedAt] datetime2 NOT NULL, [UpdatedAt] datetime2 NULL,
    [CreatedBy] uniqueidentifier NULL, [UpdatedBy] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    CONSTRAINT [PK_CtaTemplates] PRIMARY KEY ([Id])
);
GO
IF OBJECT_ID('CtaTemplates') IS NOT NULL AND NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CtaTemplates_CtaText')
    CREATE UNIQUE INDEX [IX_CtaTemplates_CtaText] ON [CtaTemplates]([CtaText]);
GO

IF OBJECT_ID('Hashtags') IS NULL
CREATE TABLE [Hashtags] (
    [Id] uniqueidentifier NOT NULL,
    [Tag] nvarchar(50) NOT NULL,
    [CreatedAt] datetime2 NOT NULL, [UpdatedAt] datetime2 NULL,
    [CreatedBy] uniqueidentifier NULL, [UpdatedBy] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    CONSTRAINT [PK_Hashtags] PRIMARY KEY ([Id])
);
GO
IF OBJECT_ID('Hashtags') IS NOT NULL AND NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Hashtags_Tag')
    CREATE UNIQUE INDEX [IX_Hashtags_Tag] ON [Hashtags]([Tag]);
GO

IF OBJECT_ID('BrandProfiles') IS NULL
CREATE TABLE [BrandProfiles] (
    [Id] uniqueidentifier NOT NULL,
    [ProjectId] uniqueidentifier NOT NULL,
    [LogoUrl] nvarchar(500) NULL,
    [BusinessName] nvarchar(150) NOT NULL,
    [BrandName] nvarchar(150) NOT NULL,
    [Industry] nvarchar(100) NULL,
    [FoundingYear] smallint NULL,
    [ShortDescription] nvarchar(1000) NULL,
    [Address] nvarchar(300) NULL,
    [ContactPhone] nvarchar(20) NULL,
    [ContactEmail] nvarchar(255) NULL,
    [Website] nvarchar(255) NULL,
    [VoiceSampleId] uniqueidentifier NULL,
    [CreatedAt] datetime2 NOT NULL, [UpdatedAt] datetime2 NULL,
    [CreatedBy] uniqueidentifier NULL, [UpdatedBy] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    CONSTRAINT [PK_BrandProfiles] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_BrandProfiles_Projects_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [Projects]([Id]),
    CONSTRAINT [FK_BrandProfiles_VoiceSampleTemplates_VoiceSampleId] FOREIGN KEY ([VoiceSampleId]) REFERENCES [VoiceSampleTemplates]([Id])
);
GO
IF OBJECT_ID('BrandProfiles') IS NOT NULL AND NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_BrandProfiles_ProjectId')
    CREATE UNIQUE INDEX [IX_BrandProfiles_ProjectId] ON [BrandProfiles]([ProjectId]);
GO

IF OBJECT_ID('BrandProfileStyles') IS NULL
CREATE TABLE [BrandProfileStyles] (
    [Id] uniqueidentifier NOT NULL,
    [BrandProfileId] uniqueidentifier NOT NULL,
    [StyleId] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL, [UpdatedAt] datetime2 NULL,
    [CreatedBy] uniqueidentifier NULL, [UpdatedBy] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    CONSTRAINT [PK_BrandProfileStyles] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_BrandProfileStyles_BrandProfiles] FOREIGN KEY ([BrandProfileId]) REFERENCES [BrandProfiles]([Id]),
    CONSTRAINT [FK_BrandProfileStyles_BrandStyles] FOREIGN KEY ([StyleId]) REFERENCES [BrandStyles]([Id])
);
GO
IF OBJECT_ID('BrandProfileStyles') IS NOT NULL AND NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_BrandProfileStyles_BrandProfileId_StyleId')
    CREATE UNIQUE INDEX [IX_BrandProfileStyles_BrandProfileId_StyleId] ON [BrandProfileStyles]([BrandProfileId],[StyleId]);
GO

IF OBJECT_ID('BrandProfileKeywords') IS NULL
CREATE TABLE [BrandProfileKeywords] (
    [Id] uniqueidentifier NOT NULL,
    [BrandProfileId] uniqueidentifier NOT NULL,
    [ToneKeywordId] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL, [UpdatedAt] datetime2 NULL,
    [CreatedBy] uniqueidentifier NULL, [UpdatedBy] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    CONSTRAINT [PK_BrandProfileKeywords] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_BrandProfileKeywords_BrandProfiles] FOREIGN KEY ([BrandProfileId]) REFERENCES [BrandProfiles]([Id]),
    CONSTRAINT [FK_BrandProfileKeywords_ToneKeywords] FOREIGN KEY ([ToneKeywordId]) REFERENCES [ToneKeywords]([Id])
);
GO
IF OBJECT_ID('BrandProfileKeywords') IS NOT NULL AND NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_BrandProfileKeywords_BrandProfileId_ToneKeywordId')
    CREATE UNIQUE INDEX [IX_BrandProfileKeywords_BrandProfileId_ToneKeywordId] ON [BrandProfileKeywords]([BrandProfileId],[ToneKeywordId]);
GO

IF OBJECT_ID('BrandCtas') IS NULL
CREATE TABLE [BrandCtas] (
    [Id] uniqueidentifier NOT NULL,
    [BrandProfileId] uniqueidentifier NOT NULL,
    [CtaId] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL, [UpdatedAt] datetime2 NULL,
    [CreatedBy] uniqueidentifier NULL, [UpdatedBy] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    CONSTRAINT [PK_BrandCtas] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_BrandCtas_BrandProfiles] FOREIGN KEY ([BrandProfileId]) REFERENCES [BrandProfiles]([Id]),
    CONSTRAINT [FK_BrandCtas_CtaTemplates] FOREIGN KEY ([CtaId]) REFERENCES [CtaTemplates]([Id])
);
GO
IF OBJECT_ID('BrandCtas') IS NOT NULL AND NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_BrandCtas_BrandProfileId_CtaId')
    CREATE UNIQUE INDEX [IX_BrandCtas_BrandProfileId_CtaId] ON [BrandCtas]([BrandProfileId],[CtaId]);
GO

IF OBJECT_ID('BrandHashtags') IS NULL
CREATE TABLE [BrandHashtags] (
    [Id] uniqueidentifier NOT NULL,
    [BrandProfileId] uniqueidentifier NOT NULL,
    [HashtagId] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL, [UpdatedAt] datetime2 NULL,
    [CreatedBy] uniqueidentifier NULL, [UpdatedBy] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    CONSTRAINT [PK_BrandHashtags] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_BrandHashtags_BrandProfiles] FOREIGN KEY ([BrandProfileId]) REFERENCES [BrandProfiles]([Id]),
    CONSTRAINT [FK_BrandHashtags_Hashtags] FOREIGN KEY ([HashtagId]) REFERENCES [Hashtags]([Id])
);
GO
IF OBJECT_ID('BrandHashtags') IS NOT NULL AND NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_BrandHashtags_BrandProfileId_HashtagId')
    CREATE UNIQUE INDEX [IX_BrandHashtags_BrandProfileId_HashtagId] ON [BrandHashtags]([BrandProfileId],[HashtagId]);
GO

IF OBJECT_ID('BrandColors') IS NULL
CREATE TABLE [BrandColors] (
    [Id] uniqueidentifier NOT NULL,
    [BrandProfileId] uniqueidentifier NOT NULL,
    [ColorHex] nvarchar(9) NOT NULL,
    [ColorType] nvarchar(20) NOT NULL DEFAULT 'Secondary',
    [SortOrder] int NOT NULL DEFAULT 0,
    [CreatedAt] datetime2 NOT NULL, [UpdatedAt] datetime2 NULL,
    [CreatedBy] uniqueidentifier NULL, [UpdatedBy] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    CONSTRAINT [PK_BrandColors] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_BrandColors_BrandProfiles] FOREIGN KEY ([BrandProfileId]) REFERENCES [BrandProfiles]([Id])
);
GO

IF OBJECT_ID('BrandFonts') IS NULL
CREATE TABLE [BrandFonts] (
    [Id] uniqueidentifier NOT NULL,
    [BrandProfileId] uniqueidentifier NOT NULL,
    [FontName] nvarchar(100) NOT NULL,
    [UsageType] nvarchar(20) NOT NULL,
    [CreatedAt] datetime2 NOT NULL, [UpdatedAt] datetime2 NULL,
    [CreatedBy] uniqueidentifier NULL, [UpdatedBy] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    CONSTRAINT [PK_BrandFonts] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_BrandFonts_BrandProfiles] FOREIGN KEY ([BrandProfileId]) REFERENCES [BrandProfiles]([Id])
);
GO

IF OBJECT_ID('ProductCategories') IS NULL
CREATE TABLE [ProductCategories] (
    [Id] uniqueidentifier NOT NULL,
    [BrandProfileId] uniqueidentifier NOT NULL,
    [CategoryName] nvarchar(100) NOT NULL,
    [CreatedAt] datetime2 NOT NULL, [UpdatedAt] datetime2 NULL,
    [CreatedBy] uniqueidentifier NULL, [UpdatedBy] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    CONSTRAINT [PK_ProductCategories] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ProductCategories_BrandProfiles] FOREIGN KEY ([BrandProfileId]) REFERENCES [BrandProfiles]([Id])
);
GO
IF OBJECT_ID('ProductCategories') IS NOT NULL AND NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ProductCategories_BrandProfileId_CategoryName')
    CREATE UNIQUE INDEX [IX_ProductCategories_BrandProfileId_CategoryName] ON [ProductCategories]([BrandProfileId],[CategoryName]);
GO

IF OBJECT_ID('Products') IS NULL
CREATE TABLE [Products] (
    [Id] uniqueidentifier NOT NULL,
    [BrandProfileId] uniqueidentifier NOT NULL,
    [ProductCategoryId] uniqueidentifier NULL,
    [ProductName] nvarchar(200) NOT NULL,
    [Price] decimal(18,2) NULL,
    [AiGeneratedDescription] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL, [UpdatedAt] datetime2 NULL,
    [CreatedBy] uniqueidentifier NULL, [UpdatedBy] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    CONSTRAINT [PK_Products] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Products_BrandProfiles] FOREIGN KEY ([BrandProfileId]) REFERENCES [BrandProfiles]([Id]),
    CONSTRAINT [FK_Products_ProductCategories] FOREIGN KEY ([ProductCategoryId]) REFERENCES [ProductCategories]([Id])
);
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_MediaFiles_Products_ProductId')
    ALTER TABLE [MediaFiles] ADD CONSTRAINT [FK_MediaFiles_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products]([Id]);
GO

/* ---------- 4) Campaign & Timeline ---------- */
IF OBJECT_ID('CampaignGoals') IS NULL
CREATE TABLE [CampaignGoals] (
    [Id] uniqueidentifier NOT NULL, [GoalName] nvarchar(100) NOT NULL,
    [CreatedAt] datetime2 NOT NULL, [UpdatedAt] datetime2 NULL,
    [CreatedBy] uniqueidentifier NULL, [UpdatedBy] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    CONSTRAINT [PK_CampaignGoals] PRIMARY KEY ([Id])
);
GO
IF OBJECT_ID('CampaignGoals') IS NOT NULL AND NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CampaignGoals_GoalName')
    CREATE UNIQUE INDEX [IX_CampaignGoals_GoalName] ON [CampaignGoals]([GoalName]);
GO

IF OBJECT_ID('PromotionTypes') IS NULL
CREATE TABLE [PromotionTypes] (
    [Id] uniqueidentifier NOT NULL, [TypeName] nvarchar(100) NOT NULL,
    [CreatedAt] datetime2 NOT NULL, [UpdatedAt] datetime2 NULL,
    [CreatedBy] uniqueidentifier NULL, [UpdatedBy] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    CONSTRAINT [PK_PromotionTypes] PRIMARY KEY ([Id])
);
GO
IF OBJECT_ID('PromotionTypes') IS NOT NULL AND NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_PromotionTypes_TypeName')
    CREATE UNIQUE INDEX [IX_PromotionTypes_TypeName] ON [PromotionTypes]([TypeName]);
GO

IF OBJECT_ID('TimelineTemplateTypes') IS NULL
CREATE TABLE [TimelineTemplateTypes] (
    [Id] uniqueidentifier NOT NULL, [TypeName] nvarchar(50) NOT NULL, [Description] nvarchar(300) NULL,
    [CreatedAt] datetime2 NOT NULL, [UpdatedAt] datetime2 NULL,
    [CreatedBy] uniqueidentifier NULL, [UpdatedBy] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    CONSTRAINT [PK_TimelineTemplateTypes] PRIMARY KEY ([Id])
);
GO
IF OBJECT_ID('TimelineTemplateTypes') IS NOT NULL AND NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TimelineTemplateTypes_TypeName')
    CREATE UNIQUE INDEX [IX_TimelineTemplateTypes_TypeName] ON [TimelineTemplateTypes]([TypeName]);
GO

IF OBJECT_ID('Campaigns') IS NULL
CREATE TABLE [Campaigns] (
    [Id] uniqueidentifier NOT NULL,
    [ProjectId] uniqueidentifier NOT NULL,
    [Name] nvarchar(200) NOT NULL,
    [GoalId] uniqueidentifier NOT NULL,
    [PromotionTypeId] uniqueidentifier NOT NULL,
    [DiscountPercent] decimal(5,2) NULL,
    [DiscountAmount] decimal(18,2) NULL,
    [MinOrderAmount] decimal(18,2) NULL,
    [StartDate] date NOT NULL,
    [EndDate] date NOT NULL,
    [NumberOfPosts] int NOT NULL DEFAULT 1,
    [Status] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL, [UpdatedAt] datetime2 NULL,
    [CreatedBy] uniqueidentifier NULL, [UpdatedBy] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    CONSTRAINT [PK_Campaigns] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Campaigns_Projects] FOREIGN KEY ([ProjectId]) REFERENCES [Projects]([Id]),
    CONSTRAINT [FK_Campaigns_CampaignGoals] FOREIGN KEY ([GoalId]) REFERENCES [CampaignGoals]([Id]),
    CONSTRAINT [FK_Campaigns_PromotionTypes] FOREIGN KEY ([PromotionTypeId]) REFERENCES [PromotionTypes]([Id]),
    CONSTRAINT [CK_Campaigns_Dates] CHECK ([EndDate] >= [StartDate]),
    CONSTRAINT [CK_Campaigns_NumberOfPosts] CHECK ([NumberOfPosts] > 0)
);
GO
IF COL_LENGTH('Campaigns', 'ProductDescription') IS NULL
    ALTER TABLE [Campaigns] ADD [ProductDescription] nvarchar(max) NULL;
GO

IF OBJECT_ID('CampaignChannelAccounts') IS NULL
CREATE TABLE [CampaignChannelAccounts] (
    [Id] uniqueidentifier NOT NULL,
    [CampaignId] uniqueidentifier NOT NULL,
    [ChannelAccountId] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL, [UpdatedAt] datetime2 NULL,
    [CreatedBy] uniqueidentifier NULL, [UpdatedBy] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    CONSTRAINT [PK_CampaignChannelAccounts] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CampaignChannelAccounts_Campaigns] FOREIGN KEY ([CampaignId]) REFERENCES [Campaigns]([Id]),
    CONSTRAINT [FK_CampaignChannelAccounts_ChannelAccounts] FOREIGN KEY ([ChannelAccountId]) REFERENCES [ChannelAccounts]([Id])
);
GO
IF OBJECT_ID('CampaignChannelAccounts') IS NOT NULL AND NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CampaignChannelAccounts_CampaignId_ChannelAccountId')
    CREATE UNIQUE INDEX [IX_CampaignChannelAccounts_CampaignId_ChannelAccountId] ON [CampaignChannelAccounts]([CampaignId],[ChannelAccountId]);
GO

IF OBJECT_ID('Timelines') IS NULL
CREATE TABLE [Timelines] (
    [Id] uniqueidentifier NOT NULL,
    [ProjectId] uniqueidentifier NOT NULL,
    [CampaignId] uniqueidentifier NULL,
    [Name] nvarchar(200) NOT NULL,
    [TemplateTypeId] uniqueidentifier NOT NULL,
    [StartDate] date NOT NULL,
    [EndDate] date NOT NULL,
    [Status] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL, [UpdatedAt] datetime2 NULL,
    [CreatedBy] uniqueidentifier NULL, [UpdatedBy] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    CONSTRAINT [PK_Timelines] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Timelines_Projects] FOREIGN KEY ([ProjectId]) REFERENCES [Projects]([Id]),
    CONSTRAINT [FK_Timelines_Campaigns] FOREIGN KEY ([CampaignId]) REFERENCES [Campaigns]([Id]),
    CONSTRAINT [FK_Timelines_TimelineTemplateTypes] FOREIGN KEY ([TemplateTypeId]) REFERENCES [TimelineTemplateTypes]([Id]),
    CONSTRAINT [CK_Timelines_Dates] CHECK ([EndDate] >= [StartDate])
);
GO

-- Bây giờ Posts.TimelineId đã có bảng đích -> thêm FK (trước đó chỉ là cột đơn lẻ chưa ràng buộc)
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Posts_Timelines_TimelineId')
    ALTER TABLE [Posts] ADD CONSTRAINT [FK_Posts_Timelines_TimelineId] FOREIGN KEY ([TimelineId]) REFERENCES [Timelines]([Id]);
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Posts_VoiceSampleTemplates_VoiceSampleId')
    ALTER TABLE [Posts] ADD CONSTRAINT [FK_Posts_VoiceSampleTemplates_VoiceSampleId] FOREIGN KEY ([VoiceSampleId]) REFERENCES [VoiceSampleTemplates]([Id]);
GO

/* ---------- 5) Đăng 1 bài lên nhiều nền tảng ---------- */
IF OBJECT_ID('PostHashtags') IS NULL
CREATE TABLE [PostHashtags] (
    [Id] uniqueidentifier NOT NULL,
    [PostId] uniqueidentifier NOT NULL,
    [HashtagId] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL, [UpdatedAt] datetime2 NULL,
    [CreatedBy] uniqueidentifier NULL, [UpdatedBy] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    CONSTRAINT [PK_PostHashtags] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PostHashtags_Posts] FOREIGN KEY ([PostId]) REFERENCES [Posts]([Id]),
    CONSTRAINT [FK_PostHashtags_Hashtags] FOREIGN KEY ([HashtagId]) REFERENCES [Hashtags]([Id])
);
GO
IF OBJECT_ID('PostHashtags') IS NOT NULL AND NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_PostHashtags_PostId_HashtagId')
    CREATE UNIQUE INDEX [IX_PostHashtags_PostId_HashtagId] ON [PostHashtags]([PostId],[HashtagId]);
GO

IF OBJECT_ID('PostChannelAccounts') IS NULL
CREATE TABLE [PostChannelAccounts] (
    [Id] uniqueidentifier NOT NULL,
    [PostId] uniqueidentifier NOT NULL,
    [ChannelAccountId] uniqueidentifier NOT NULL,
    [Status] int NOT NULL,
    [ExternalPostId] nvarchar(128) NULL,
    [PublishedUrl] nvarchar(1000) NULL,
    [PublishedAt] datetime2 NULL,
    [CreatedAt] datetime2 NOT NULL, [UpdatedAt] datetime2 NULL,
    [CreatedBy] uniqueidentifier NULL, [UpdatedBy] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    CONSTRAINT [PK_PostChannelAccounts] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PostChannelAccounts_Posts] FOREIGN KEY ([PostId]) REFERENCES [Posts]([Id]),
    CONSTRAINT [FK_PostChannelAccounts_ChannelAccounts] FOREIGN KEY ([ChannelAccountId]) REFERENCES [ChannelAccounts]([Id])
);
GO
IF OBJECT_ID('PostChannelAccounts') IS NOT NULL AND NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_PostChannelAccounts_PostId_ChannelAccountId')
    CREATE UNIQUE INDEX [IX_PostChannelAccounts_PostId_ChannelAccountId] ON [PostChannelAccounts]([PostId],[ChannelAccountId]);
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_PostSchedules_PostChannelAccounts_PostChannelAccountId')
    ALTER TABLE [PostSchedules] ADD CONSTRAINT [FK_PostSchedules_PostChannelAccounts_PostChannelAccountId]
        FOREIGN KEY ([PostChannelAccountId]) REFERENCES [PostChannelAccounts]([Id]);
GO

IF OBJECT_ID('PostAnalytics') IS NULL
CREATE TABLE [PostAnalytics] (
    [Id] uniqueidentifier NOT NULL,
    [PostChannelAccountId] uniqueidentifier NOT NULL,
    [Reach] int NOT NULL DEFAULT 0,
    [Likes] int NOT NULL DEFAULT 0,
    [Comments] int NOT NULL DEFAULT 0,
    [Shares] int NOT NULL DEFAULT 0,
    [RecordedAt] datetime2 NOT NULL,
    [CreatedAt] datetime2 NOT NULL, [UpdatedAt] datetime2 NULL,
    [CreatedBy] uniqueidentifier NULL, [UpdatedBy] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    CONSTRAINT [PK_PostAnalytics] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PostAnalytics_PostChannelAccounts] FOREIGN KEY ([PostChannelAccountId]) REFERENCES [PostChannelAccounts]([Id])
);
GO
IF OBJECT_ID('PostAnalytics') IS NOT NULL AND NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_PostAnalytics_PostChannelAccountId')
    CREATE UNIQUE INDEX [IX_PostAnalytics_PostChannelAccountId] ON [PostAnalytics]([PostChannelAccountId]);
GO

/* ---------- 6) Seed dữ liệu lookup cơ bản (khớp đúng UI FlowMate) ---------- */
IF NOT EXISTS (SELECT * FROM [BrandStyles])
INSERT INTO [BrandStyles] ([Id],[StyleName],[CreatedAt],[IsDeleted]) VALUES
    (NEWID(),N'Streetwear',SYSUTCDATETIME(),0),(NEWID(),N'Y2K',SYSUTCDATETIME(),0),
    (NEWID(),N'Basic',SYSUTCDATETIME(),0),(NEWID(),N'Nữ tính',SYSUTCDATETIME(),0),
    (NEWID(),N'Công sở',SYSUTCDATETIME(),0),(NEWID(),N'Vintage',SYSUTCDATETIME(),0),
    (NEWID(),N'Boho',SYSUTCDATETIME(),0),(NEWID(),N'Sang trọng',SYSUTCDATETIME(),0),
    (NEWID(),N'Thể thao',SYSUTCDATETIME(),0),(NEWID(),N'Dạo phố',SYSUTCDATETIME(),0),
    (NEWID(),N'Tomboy',SYSUTCDATETIME(),0),(NEWID(),N'Dễ thương',SYSUTCDATETIME(),0),
    (NEWID(),N'Unisex',SYSUTCDATETIME(),0),(NEWID(),N'Local brand',SYSUTCDATETIME(),0);
GO

IF NOT EXISTS (SELECT * FROM [ToneKeywords])
INSERT INTO [ToneKeywords] ([Id],[Keyword],[CreatedAt],[IsDeleted]) VALUES
    (NEWID(),N'Gần gũi',SYSUTCDATETIME(),0),(NEWID(),N'Dí dỏm',SYSUTCDATETIME(),0),
    (NEWID(),N'Trẻ trung',SYSUTCDATETIME(),0),(NEWID(),N'Sang trọng',SYSUTCDATETIME(),0),
    (NEWID(),N'Nghiêm túc',SYSUTCDATETIME(),0),(NEWID(),N'Trưởng thành',SYSUTCDATETIME(),0),
    (NEWID(),N'Nhiệt tình',SYSUTCDATETIME(),0),(NEWID(),N'Điềm tĩnh',SYSUTCDATETIME(),0),
    (NEWID(),N'Táo bạo',SYSUTCDATETIME(),0),(NEWID(),N'Kín đáo',SYSUTCDATETIME(),0),
    (NEWID(),N'Chân thành',SYSUTCDATETIME(),0),(NEWID(),N'Quyết đoán',SYSUTCDATETIME(),0),
    (NEWID(),N'Ấm áp',SYSUTCDATETIME(),0),(NEWID(),N'Tối giản',SYSUTCDATETIME(),0),
    (NEWID(),N'Bí ẩn',SYSUTCDATETIME(),0);
GO

IF NOT EXISTS (SELECT * FROM [VoiceSampleTemplates])
INSERT INTO [VoiceSampleTemplates] ([Id],[SampleText],[CreatedAt],[IsDeleted]) VALUES
    (NEWID(),N'Hàng mới về nè cả nhà ơi, xinh xỉu luôn á 🐱',SYSUTCDATETIME(),0),
    (NEWID(),N'Bộ sưu tập mới đã có mặt tại cửa hàng.',SYSUTCDATETIME(),0),
    (NEWID(),N'Chào đón thiết kế mới — tinh giản, tinh tế, dành cho phái đẹp hiện đại.',SYSUTCDATETIME(),0),
    (NEWID(),N'Ê mấy bạn, đợt này về hàng chất lắm, lướt xuống coi liền tay nè!',SYSUTCDATETIME(),0),
    (NEWID(),N'Item của tuần: form dáng chuẩn, chất liệu bền, phối được với mọi outfit.',SYSUTCDATETIME(),0),
    (NEWID(),N'Một thiết kế, nhiều câu chuyện. Khám phá bộ sưu tập giới hạn ngay hôm nay.',SYSUTCDATETIME(),0);
GO

IF NOT EXISTS (SELECT * FROM [CtaTemplates])
INSERT INTO [CtaTemplates] ([Id],[CtaText],[CreatedAt],[IsDeleted]) VALUES
    (NEWID(),N'Chốt đơn ngay',SYSUTCDATETIME(),0),
    (NEWID(),N'Inbox tư vấn',SYSUTCDATETIME(),0),
    (NEWID(),N'Đặt hàng liền tay',SYSUTCDATETIME(),0);
GO

IF NOT EXISTS (SELECT * FROM [CampaignGoals])
INSERT INTO [CampaignGoals] ([Id],[GoalName],[CreatedAt],[IsDeleted]) VALUES
    (NEWID(),N'Ra mắt sản phẩm mới',SYSUTCDATETIME(),0),
    (NEWID(),N'Bán bộ sưu tập theo mùa',SYSUTCDATETIME(),0),
    (NEWID(),N'Xả hàng tồn kho',SYSUTCDATETIME(),0),
    (NEWID(),N'Flash sale chớp nhoáng',SYSUTCDATETIME(),0),
    (NEWID(),N'Tăng nhận diện thương hiệu',SYSUTCDATETIME(),0),
    (NEWID(),N'Tăng tương tác - follow',SYSUTCDATETIME(),0),
    (NEWID(),N'Giữ chân khách cũ',SYSUTCDATETIME(),0),
    (NEWID(),N'Mừng dịp lễ - sự kiện',SYSUTCDATETIME(),0);
GO

IF NOT EXISTS (SELECT * FROM [PromotionTypes])
INSERT INTO [PromotionTypes] ([Id],[TypeName],[CreatedAt],[IsDeleted]) VALUES
    (NEWID(),N'Giảm %',SYSUTCDATETIME(),0),
    (NEWID(),N'Giảm số tiền cố định',SYSUTCDATETIME(),0),
    (NEWID(),N'Mua X tặng Y',SYSUTCDATETIME(),0),
    (NEWID(),N'Freeship',SYSUTCDATETIME(),0),
    (NEWID(),N'Quà tặng kèm',SYSUTCDATETIME(),0),
    (NEWID(),N'Giảm giá theo combo',SYSUTCDATETIME(),0),
    (NEWID(),N'Mã voucher riêng',SYSUTCDATETIME(),0),
    (NEWID(),N'Không có ưu đãi',SYSUTCDATETIME(),0);
GO

IF NOT EXISTS (SELECT * FROM [TimelineTemplateTypes])
INSERT INTO [TimelineTemplateTypes] ([Id],[TypeName],[Description],[CreatedAt],[IsDeleted]) VALUES
    (NEWID(),N'Classic Campaign',N'Timeline theo từng ngày, phù hợp chiến dịch marketing, ra mắt sản phẩm, khuyến mãi.',SYSUTCDATETIME(),0),
    (NEWID(),N'Roadmap',N'Timeline theo từng giai đoạn, phù hợp kế hoạch dài hạn, roadmap dự án.',SYSUTCDATETIME(),0),
    (NEWID(),N'Calendar',N'Timeline theo lịch thực tế, phù hợp lịch sự kiện, deadline, hoạt động theo tháng.',SYSUTCDATETIME(),0);
GO
