IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE TABLE [Channels] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [Code] nvarchar(50) NOT NULL,
        [Description] nvarchar(500) NULL,
        [IconUrl] nvarchar(500) NULL,
        [IsActive] bit NOT NULL,
        [SortOrder] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK_Channels] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE TABLE [Permissions] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Code] nvarchar(100) NOT NULL,
        [Description] nvarchar(500) NULL,
        [Module] nvarchar(100) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK_Permissions] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE TABLE [Plans] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [Description] nvarchar(1000) NULL,
        [Code] nvarchar(50) NOT NULL,
        [Price] decimal(18,2) NOT NULL,
        [BillingPeriod] int NOT NULL,
        [MaxProjects] int NOT NULL,
        [MaxChannels] int NOT NULL,
        [MaxPostsPerMonth] int NOT NULL,
        [CreditsIncluded] int NOT NULL,
        [IsActive] bit NOT NULL,
        [SortOrder] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK_Plans] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE TABLE [Roles] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [Description] nvarchar(500) NULL,
        [IsSystem] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK_Roles] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE TABLE [Settings] (
        [Id] uniqueidentifier NOT NULL,
        [Key] nvarchar(200) NOT NULL,
        [Value] nvarchar(max) NOT NULL,
        [Category] nvarchar(100) NULL,
        [Description] nvarchar(500) NULL,
        [IsPublic] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK_Settings] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE TABLE [Users] (
        [Id] uniqueidentifier NOT NULL,
        [Email] nvarchar(256) NOT NULL,
        [PasswordHash] nvarchar(512) NOT NULL,
        [FirstName] nvarchar(100) NOT NULL,
        [LastName] nvarchar(100) NOT NULL,
        [AvatarUrl] nvarchar(500) NULL,
        [Phone] nvarchar(20) NULL,
        [IsActive] bit NOT NULL,
        [LastLoginAt] datetime2 NULL,
        [GoogleId] nvarchar(128) NULL,
        [FacebookId] nvarchar(128) NULL,
        [ReferralCode] nvarchar(32) NULL,
        [ReferredByUserId] uniqueidentifier NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK_Users] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Users_Users_ReferredByUserId] FOREIGN KEY ([ReferredByUserId]) REFERENCES [Users] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE TABLE [RolePermissions] (
        [Id] uniqueidentifier NOT NULL,
        [RoleId] uniqueidentifier NOT NULL,
        [PermissionId] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK_RolePermissions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RolePermissions_Permissions_PermissionId] FOREIGN KEY ([PermissionId]) REFERENCES [Permissions] ([Id]),
        CONSTRAINT [FK_RolePermissions_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Roles] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE TABLE [Affiliates] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [Code] nvarchar(32) NOT NULL,
        [CommissionRate] decimal(5,4) NOT NULL,
        [TotalEarnings] decimal(18,2) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK_Affiliates] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Affiliates_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE TABLE [AuditLogs] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NULL,
        [Action] nvarchar(100) NOT NULL,
        [EntityType] nvarchar(200) NOT NULL,
        [EntityId] uniqueidentifier NOT NULL,
        [OldValues] nvarchar(max) NULL,
        [NewValues] nvarchar(max) NULL,
        [IpAddress] nvarchar(45) NULL,
        [UserAgent] nvarchar(500) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK_AuditLogs] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AuditLogs_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE TABLE [Credits] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [Balance] int NOT NULL,
        [TotalEarned] int NOT NULL,
        [TotalUsed] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK_Credits] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Credits_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE TABLE [FacebookAccounts] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [FacebookUserId] nvarchar(128) NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Email] nvarchar(256) NULL,
        [ProfilePictureUrl] nvarchar(500) NULL,
        [IsActive] bit NOT NULL,
        [TokenExpiresAt] datetime2 NULL,
        [LastSyncedAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK_FacebookAccounts] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_FacebookAccounts_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE TABLE [Notifications] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [Title] nvarchar(200) NOT NULL,
        [Message] nvarchar(2000) NOT NULL,
        [Type] int NOT NULL,
        [IsRead] bit NOT NULL,
        [ReadAt] datetime2 NULL,
        [ReferenceType] nvarchar(100) NULL,
        [ReferenceId] uniqueidentifier NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK_Notifications] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Notifications_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE TABLE [PasswordResetTokens] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [Token] nvarchar(512) NOT NULL,
        [OtpCode] nvarchar(max) NOT NULL,
        [ExpiresAt] datetime2 NOT NULL,
        [UsedAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK_PasswordResetTokens] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PasswordResetTokens_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE TABLE [Projects] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Description] nvarchar(2000) NULL,
        [LogoUrl] nvarchar(500) NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK_Projects] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Projects_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE TABLE [RefreshTokens] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [Token] nvarchar(512) NOT NULL,
        [ExpiresAt] datetime2 NOT NULL,
        [RevokedAt] datetime2 NULL,
        [ReplacedByToken] nvarchar(512) NULL,
        [CreatedByIp] nvarchar(max) NULL,
        [IsRevoked] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK_RefreshTokens] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RefreshTokens_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE TABLE [Subscriptions] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [PlanId] uniqueidentifier NOT NULL,
        [Status] int NOT NULL,
        [StartDate] datetime2 NOT NULL,
        [EndDate] datetime2 NULL,
        [CancelledAt] datetime2 NULL,
        [AutoRenew] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK_Subscriptions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Subscriptions_Plans_PlanId] FOREIGN KEY ([PlanId]) REFERENCES [Plans] ([Id]),
        CONSTRAINT [FK_Subscriptions_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE TABLE [UserRoles] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [RoleId] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK_UserRoles] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_UserRoles_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Roles] ([Id]),
        CONSTRAINT [FK_UserRoles_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE TABLE [WordPressSites] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [SiteUrl] nvarchar(500) NOT NULL,
        [SiteName] nvarchar(200) NOT NULL,
        [Username] nvarchar(100) NOT NULL,
        [ApplicationPassword] nvarchar(max) NOT NULL,
        [IsWooCommerce] bit NOT NULL,
        [IsConnected] bit NOT NULL,
        [LastSyncedAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK_WordPressSites] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_WordPressSites_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE TABLE [ZaloAccounts] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [ZaloUserId] nvarchar(128) NOT NULL,
        [DisplayName] nvarchar(200) NOT NULL,
        [AvatarUrl] nvarchar(500) NULL,
        [IsConnected] bit NOT NULL,
        [TokenExpiresAt] datetime2 NULL,
        [LastSyncedAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK_ZaloAccounts] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ZaloAccounts_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE TABLE [AffiliateLinks] (
        [Id] uniqueidentifier NOT NULL,
        [AffiliateId] uniqueidentifier NOT NULL,
        [Url] nvarchar(1000) NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [Campaign] nvarchar(100) NULL,
        [ClickCount] int NOT NULL,
        [ConversionCount] int NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK_AffiliateLinks] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AffiliateLinks_Affiliates_AffiliateId] FOREIGN KEY ([AffiliateId]) REFERENCES [Affiliates] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE TABLE [CreditTransactions] (
        [Id] uniqueidentifier NOT NULL,
        [CreditId] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [Type] int NOT NULL,
        [Amount] int NOT NULL,
        [BalanceAfter] int NOT NULL,
        [Description] nvarchar(500) NULL,
        [ReferenceType] nvarchar(100) NULL,
        [ReferenceId] uniqueidentifier NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK_CreditTransactions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CreditTransactions_Credits_CreditId] FOREIGN KEY ([CreditId]) REFERENCES [Credits] ([Id]),
        CONSTRAINT [FK_CreditTransactions_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE TABLE [FacebookPages] (
        [Id] uniqueidentifier NOT NULL,
        [FacebookAccountId] uniqueidentifier NOT NULL,
        [PageId] nvarchar(128) NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Category] nvarchar(100) NULL,
        [ProfilePictureUrl] nvarchar(500) NULL,
        [IsConnected] bit NOT NULL,
        [LastSyncedAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK_FacebookPages] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_FacebookPages_FacebookAccounts_FacebookAccountId] FOREIGN KEY ([FacebookAccountId]) REFERENCES [FacebookAccounts] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE TABLE [AiPrompts] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NULL,
        [ProjectId] uniqueidentifier NULL,
        [Name] nvarchar(200) NOT NULL,
        [Template] nvarchar(max) NOT NULL,
        [Category] nvarchar(100) NULL,
        [IsSystem] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK_AiPrompts] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AiPrompts_Projects_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [Projects] ([Id]),
        CONSTRAINT [FK_AiPrompts_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE TABLE [ChannelAccounts] (
        [Id] uniqueidentifier NOT NULL,
        [ProjectId] uniqueidentifier NOT NULL,
        [ChannelId] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [ExternalId] nvarchar(128) NULL,
        [ProfileUrl] nvarchar(500) NULL,
        [AvatarUrl] nvarchar(500) NULL,
        [IsActive] bit NOT NULL,
        [LastSyncedAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK_ChannelAccounts] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ChannelAccounts_Channels_ChannelId] FOREIGN KEY ([ChannelId]) REFERENCES [Channels] ([Id]),
        CONSTRAINT [FK_ChannelAccounts_Projects_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [Projects] ([Id]),
        CONSTRAINT [FK_ChannelAccounts_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE TABLE [MediaFiles] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [ProjectId] uniqueidentifier NULL,
        [FileName] nvarchar(255) NOT NULL,
        [FileUrl] nvarchar(1000) NOT NULL,
        [MimeType] nvarchar(100) NOT NULL,
        [FileSize] bigint NOT NULL,
        [ThumbnailUrl] nvarchar(1000) NULL,
        [Width] int NULL,
        [Height] int NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK_MediaFiles] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_MediaFiles_Projects_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [Projects] ([Id]),
        CONSTRAINT [FK_MediaFiles_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE TABLE [AffiliateCommissions] (
        [Id] uniqueidentifier NOT NULL,
        [AffiliateId] uniqueidentifier NOT NULL,
        [ReferredUserId] uniqueidentifier NOT NULL,
        [SubscriptionId] uniqueidentifier NULL,
        [Amount] decimal(18,2) NOT NULL,
        [Status] int NOT NULL,
        [PaidAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK_AffiliateCommissions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AffiliateCommissions_Affiliates_AffiliateId] FOREIGN KEY ([AffiliateId]) REFERENCES [Affiliates] ([Id]),
        CONSTRAINT [FK_AffiliateCommissions_Subscriptions_SubscriptionId] FOREIGN KEY ([SubscriptionId]) REFERENCES [Subscriptions] ([Id]),
        CONSTRAINT [FK_AffiliateCommissions_Users_ReferredUserId] FOREIGN KEY ([ReferredUserId]) REFERENCES [Users] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE TABLE [Invoices] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [SubscriptionId] uniqueidentifier NULL,
        [InvoiceNumber] nvarchar(50) NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [Tax] decimal(18,2) NOT NULL,
        [Total] decimal(18,2) NOT NULL,
        [Status] int NOT NULL,
        [DueDate] datetime2 NOT NULL,
        [PaidAt] datetime2 NULL,
        [Notes] nvarchar(1000) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK_Invoices] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Invoices_Subscriptions_SubscriptionId] FOREIGN KEY ([SubscriptionId]) REFERENCES [Subscriptions] ([Id]),
        CONSTRAINT [FK_Invoices_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE TABLE [WordPressPosts] (
        [Id] uniqueidentifier NOT NULL,
        [WordPressSiteId] uniqueidentifier NOT NULL,
        [ExternalPostId] nvarchar(128) NOT NULL,
        [Title] nvarchar(500) NOT NULL,
        [Excerpt] nvarchar(2000) NULL,
        [Status] int NOT NULL,
        [Permalink] nvarchar(1000) NULL,
        [PublishedAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK_WordPressPosts] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_WordPressPosts_WordPressSites_WordPressSiteId] FOREIGN KEY ([WordPressSiteId]) REFERENCES [WordPressSites] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE TABLE [ZaloPosts] (
        [Id] uniqueidentifier NOT NULL,
        [ZaloAccountId] uniqueidentifier NOT NULL,
        [ExternalPostId] nvarchar(128) NULL,
        [Content] nvarchar(max) NOT NULL,
        [Status] int NOT NULL,
        [PublishedAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK_ZaloPosts] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ZaloPosts_ZaloAccounts_ZaloAccountId] FOREIGN KEY ([ZaloAccountId]) REFERENCES [ZaloAccounts] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE TABLE [AiGeneratedContents] (
        [Id] uniqueidentifier NOT NULL,
        [AiPromptId] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [ProjectId] uniqueidentifier NULL,
        [Input] nvarchar(max) NOT NULL,
        [Output] nvarchar(max) NULL,
        [TokensUsed] int NOT NULL,
        [Status] int NOT NULL,
        [ErrorMessage] nvarchar(2000) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK_AiGeneratedContents] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AiGeneratedContents_AiPrompts_AiPromptId] FOREIGN KEY ([AiPromptId]) REFERENCES [AiPrompts] ([Id]),
        CONSTRAINT [FK_AiGeneratedContents_Projects_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [Projects] ([Id]),
        CONSTRAINT [FK_AiGeneratedContents_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE TABLE [Posts] (
        [Id] uniqueidentifier NOT NULL,
        [ProjectId] uniqueidentifier NOT NULL,
        [ChannelAccountId] uniqueidentifier NOT NULL,
        [Title] nvarchar(500) NOT NULL,
        [Status] int NOT NULL,
        [ExternalPostId] nvarchar(128) NULL,
        [PublishedUrl] nvarchar(1000) NULL,
        [PublishedAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK_Posts] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Posts_ChannelAccounts_ChannelAccountId] FOREIGN KEY ([ChannelAccountId]) REFERENCES [ChannelAccounts] ([Id]),
        CONSTRAINT [FK_Posts_Projects_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [Projects] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE TABLE [Payments] (
        [Id] uniqueidentifier NOT NULL,
        [InvoiceId] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [PaymentMethod] int NOT NULL,
        [TransactionId] nvarchar(128) NULL,
        [Status] int NOT NULL,
        [PaidAt] datetime2 NULL,
        [FailureReason] nvarchar(1000) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK_Payments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Payments_Invoices_InvoiceId] FOREIGN KEY ([InvoiceId]) REFERENCES [Invoices] ([Id]),
        CONSTRAINT [FK_Payments_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE TABLE [PostContents] (
        [Id] uniqueidentifier NOT NULL,
        [PostId] uniqueidentifier NOT NULL,
        [ContentType] int NOT NULL,
        [Content] nvarchar(max) NOT NULL,
        [SortOrder] int NOT NULL,
        [MediaFileId] uniqueidentifier NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK_PostContents] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PostContents_MediaFiles_MediaFileId] FOREIGN KEY ([MediaFileId]) REFERENCES [MediaFiles] ([Id]),
        CONSTRAINT [FK_PostContents_Posts_PostId] FOREIGN KEY ([PostId]) REFERENCES [Posts] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE TABLE [PostLogs] (
        [Id] uniqueidentifier NOT NULL,
        [PostId] uniqueidentifier NOT NULL,
        [Action] nvarchar(100) NOT NULL,
        [Message] nvarchar(2000) NOT NULL,
        [Level] int NOT NULL,
        [Details] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK_PostLogs] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PostLogs_Posts_PostId] FOREIGN KEY ([PostId]) REFERENCES [Posts] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE TABLE [PostSchedules] (
        [Id] uniqueidentifier NOT NULL,
        [PostId] uniqueidentifier NOT NULL,
        [ScheduledAt] datetime2 NOT NULL,
        [Status] int NOT NULL,
        [ExecutedAt] datetime2 NULL,
        [FailureReason] nvarchar(1000) NULL,
        [RetryCount] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK_PostSchedules] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PostSchedules_Posts_PostId] FOREIGN KEY ([PostId]) REFERENCES [Posts] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AffiliateCommissions_AffiliateId] ON [AffiliateCommissions] ([AffiliateId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AffiliateCommissions_IsDeleted] ON [AffiliateCommissions] ([IsDeleted]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AffiliateCommissions_ReferredUserId] ON [AffiliateCommissions] ([ReferredUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AffiliateCommissions_Status] ON [AffiliateCommissions] ([Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AffiliateCommissions_SubscriptionId] ON [AffiliateCommissions] ([SubscriptionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AffiliateLinks_AffiliateId] ON [AffiliateLinks] ([AffiliateId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AffiliateLinks_IsDeleted] ON [AffiliateLinks] ([IsDeleted]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Affiliates_Code] ON [Affiliates] ([Code]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Affiliates_IsDeleted] ON [Affiliates] ([IsDeleted]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Affiliates_UserId] ON [Affiliates] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AiGeneratedContents_AiPromptId] ON [AiGeneratedContents] ([AiPromptId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AiGeneratedContents_CreatedAt] ON [AiGeneratedContents] ([CreatedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AiGeneratedContents_IsDeleted] ON [AiGeneratedContents] ([IsDeleted]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AiGeneratedContents_ProjectId] ON [AiGeneratedContents] ([ProjectId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AiGeneratedContents_UserId] ON [AiGeneratedContents] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AiPrompts_IsDeleted] ON [AiPrompts] ([IsDeleted]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AiPrompts_IsSystem] ON [AiPrompts] ([IsSystem]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AiPrompts_ProjectId] ON [AiPrompts] ([ProjectId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AiPrompts_UserId] ON [AiPrompts] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AuditLogs_CreatedAt] ON [AuditLogs] ([CreatedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AuditLogs_EntityType] ON [AuditLogs] ([EntityType]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AuditLogs_IsDeleted] ON [AuditLogs] ([IsDeleted]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AuditLogs_UserId] ON [AuditLogs] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ChannelAccounts_ChannelId] ON [ChannelAccounts] ([ChannelId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ChannelAccounts_IsDeleted] ON [ChannelAccounts] ([IsDeleted]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ChannelAccounts_ProjectId_ChannelId_ExternalId] ON [ChannelAccounts] ([ProjectId], [ChannelId], [ExternalId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ChannelAccounts_UserId] ON [ChannelAccounts] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Channels_Code] ON [Channels] ([Code]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Channels_IsDeleted] ON [Channels] ([IsDeleted]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Credits_IsDeleted] ON [Credits] ([IsDeleted]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Credits_UserId] ON [Credits] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CreditTransactions_CreatedAt] ON [CreditTransactions] ([CreatedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CreditTransactions_CreditId] ON [CreditTransactions] ([CreditId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CreditTransactions_IsDeleted] ON [CreditTransactions] ([IsDeleted]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CreditTransactions_UserId] ON [CreditTransactions] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_FacebookAccounts_FacebookUserId] ON [FacebookAccounts] ([FacebookUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_FacebookAccounts_IsDeleted] ON [FacebookAccounts] ([IsDeleted]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_FacebookAccounts_UserId] ON [FacebookAccounts] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_FacebookPages_FacebookAccountId] ON [FacebookPages] ([FacebookAccountId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_FacebookPages_IsDeleted] ON [FacebookPages] ([IsDeleted]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_FacebookPages_PageId] ON [FacebookPages] ([PageId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Invoices_InvoiceNumber] ON [Invoices] ([InvoiceNumber]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Invoices_IsDeleted] ON [Invoices] ([IsDeleted]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Invoices_Status] ON [Invoices] ([Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Invoices_SubscriptionId] ON [Invoices] ([SubscriptionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Invoices_UserId] ON [Invoices] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_MediaFiles_FileUrl] ON [MediaFiles] ([FileUrl]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_MediaFiles_IsDeleted] ON [MediaFiles] ([IsDeleted]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_MediaFiles_ProjectId] ON [MediaFiles] ([ProjectId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_MediaFiles_UserId] ON [MediaFiles] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Notifications_CreatedAt] ON [Notifications] ([CreatedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Notifications_IsDeleted] ON [Notifications] ([IsDeleted]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Notifications_UserId_IsRead] ON [Notifications] ([UserId], [IsRead]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PasswordResetTokens_IsDeleted] ON [PasswordResetTokens] ([IsDeleted]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PasswordResetTokens_Token] ON [PasswordResetTokens] ([Token]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PasswordResetTokens_UserId] ON [PasswordResetTokens] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Payments_InvoiceId] ON [Payments] ([InvoiceId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Payments_IsDeleted] ON [Payments] ([IsDeleted]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    EXEC(N'CREATE INDEX [IX_Payments_TransactionId] ON [Payments] ([TransactionId]) WHERE [TransactionId] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Payments_UserId] ON [Payments] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Permissions_Code] ON [Permissions] ([Code]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Permissions_IsDeleted] ON [Permissions] ([IsDeleted]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Plans_Code] ON [Plans] ([Code]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Plans_IsActive] ON [Plans] ([IsActive]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Plans_IsDeleted] ON [Plans] ([IsDeleted]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Plans_Name] ON [Plans] ([Name]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Plans_SortOrder] ON [Plans] ([SortOrder]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PostContents_IsDeleted] ON [PostContents] ([IsDeleted]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PostContents_MediaFileId] ON [PostContents] ([MediaFileId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PostContents_PostId_SortOrder] ON [PostContents] ([PostId], [SortOrder]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PostLogs_CreatedAt] ON [PostLogs] ([CreatedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PostLogs_IsDeleted] ON [PostLogs] ([IsDeleted]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PostLogs_PostId] ON [PostLogs] ([PostId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Posts_ChannelAccountId] ON [Posts] ([ChannelAccountId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Posts_IsDeleted] ON [Posts] ([IsDeleted]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Posts_ProjectId] ON [Posts] ([ProjectId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Posts_PublishedAt] ON [Posts] ([PublishedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Posts_Status] ON [Posts] ([Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PostSchedules_IsDeleted] ON [PostSchedules] ([IsDeleted]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PostSchedules_PostId] ON [PostSchedules] ([PostId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PostSchedules_Status_ScheduledAt] ON [PostSchedules] ([Status], [ScheduledAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Projects_IsDeleted] ON [Projects] ([IsDeleted]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Projects_UserId] ON [Projects] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RefreshTokens_IsDeleted] ON [RefreshTokens] ([IsDeleted]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_RefreshTokens_Token] ON [RefreshTokens] ([Token]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RefreshTokens_UserId_ExpiresAt] ON [RefreshTokens] ([UserId], [ExpiresAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RolePermissions_IsDeleted] ON [RolePermissions] ([IsDeleted]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RolePermissions_PermissionId] ON [RolePermissions] ([PermissionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_RolePermissions_RoleId_PermissionId] ON [RolePermissions] ([RoleId], [PermissionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Roles_IsDeleted] ON [Roles] ([IsDeleted]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Roles_Name] ON [Roles] ([Name]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Settings_Category] ON [Settings] ([Category]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Settings_IsDeleted] ON [Settings] ([IsDeleted]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Settings_Key] ON [Settings] ([Key]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Subscriptions_IsDeleted] ON [Subscriptions] ([IsDeleted]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Subscriptions_PlanId] ON [Subscriptions] ([PlanId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Subscriptions_UserId_Status] ON [Subscriptions] ([UserId], [Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_UserRoles_IsDeleted] ON [UserRoles] ([IsDeleted]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_UserRoles_RoleId] ON [UserRoles] ([RoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_UserRoles_UserId_RoleId] ON [UserRoles] ([UserId], [RoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    EXEC(N'CREATE INDEX [IX_Users_FacebookId] ON [Users] ([FacebookId]) WHERE [FacebookId] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    EXEC(N'CREATE INDEX [IX_Users_GoogleId] ON [Users] ([GoogleId]) WHERE [GoogleId] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Users_IsDeleted] ON [Users] ([IsDeleted]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Users_ReferralCode] ON [Users] ([ReferralCode]) WHERE [ReferralCode] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Users_ReferredByUserId] ON [Users] ([ReferredByUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_WordPressPosts_IsDeleted] ON [WordPressPosts] ([IsDeleted]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_WordPressPosts_Status] ON [WordPressPosts] ([Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_WordPressPosts_WordPressSiteId] ON [WordPressPosts] ([WordPressSiteId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_WordPressSites_IsDeleted] ON [WordPressSites] ([IsDeleted]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_WordPressSites_UserId] ON [WordPressSites] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ZaloAccounts_IsDeleted] ON [ZaloAccounts] ([IsDeleted]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ZaloAccounts_UserId] ON [ZaloAccounts] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ZaloAccounts_ZaloUserId] ON [ZaloAccounts] ([ZaloUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ZaloPosts_IsDeleted] ON [ZaloPosts] ([IsDeleted]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ZaloPosts_Status] ON [ZaloPosts] ([Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ZaloPosts_ZaloAccountId] ON [ZaloPosts] ([ZaloAccountId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618173606_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260618173606_InitialCreate', N'8.0.11');
END;
GO

COMMIT;
GO

