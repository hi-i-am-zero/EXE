-- =============================================
-- AutoWork SaaS Database Script
-- SQL Server 2019+
-- =============================================

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'AutoWork')
BEGIN
    CREATE DATABASE [AutoWork];
END
GO

USE [AutoWork];
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

-- =============================================
-- Drop existing tables (reverse dependency order)
-- =============================================
IF OBJECT_ID(N'dbo.PasswordResetTokens', N'U') IS NOT NULL DROP TABLE dbo.PasswordResetTokens;
IF OBJECT_ID(N'dbo.RefreshTokens', N'U') IS NOT NULL DROP TABLE dbo.RefreshTokens;
IF OBJECT_ID(N'dbo.AuditLogs', N'U') IS NOT NULL DROP TABLE dbo.AuditLogs;
IF OBJECT_ID(N'dbo.Payments', N'U') IS NOT NULL DROP TABLE dbo.Payments;
IF OBJECT_ID(N'dbo.Invoices', N'U') IS NOT NULL DROP TABLE dbo.Invoices;
IF OBJECT_ID(N'dbo.AffiliateCommissions', N'U') IS NOT NULL DROP TABLE dbo.AffiliateCommissions;
IF OBJECT_ID(N'dbo.AffiliateLinks', N'U') IS NOT NULL DROP TABLE dbo.AffiliateLinks;
IF OBJECT_ID(N'dbo.Affiliates', N'U') IS NOT NULL DROP TABLE dbo.Affiliates;
IF OBJECT_ID(N'dbo.Notifications', N'U') IS NOT NULL DROP TABLE dbo.Notifications;
IF OBJECT_ID(N'dbo.PostLogs', N'U') IS NOT NULL DROP TABLE dbo.PostLogs;
IF OBJECT_ID(N'dbo.PostSchedules', N'U') IS NOT NULL DROP TABLE dbo.PostSchedules;
IF OBJECT_ID(N'dbo.PostContents', N'U') IS NOT NULL DROP TABLE dbo.PostContents;
IF OBJECT_ID(N'dbo.Posts', N'U') IS NOT NULL DROP TABLE dbo.Posts;
IF OBJECT_ID(N'dbo.ZaloPosts', N'U') IS NOT NULL DROP TABLE dbo.ZaloPosts;
IF OBJECT_ID(N'dbo.ZaloAccounts', N'U') IS NOT NULL DROP TABLE dbo.ZaloAccounts;
IF OBJECT_ID(N'dbo.WordPressPosts', N'U') IS NOT NULL DROP TABLE dbo.WordPressPosts;
IF OBJECT_ID(N'dbo.WordPressSites', N'U') IS NOT NULL DROP TABLE dbo.WordPressSites;
IF OBJECT_ID(N'dbo.FacebookPages', N'U') IS NOT NULL DROP TABLE dbo.FacebookPages;
IF OBJECT_ID(N'dbo.FacebookAccounts', N'U') IS NOT NULL DROP TABLE dbo.FacebookAccounts;
IF OBJECT_ID(N'dbo.AiGeneratedContents', N'U') IS NOT NULL DROP TABLE dbo.AiGeneratedContents;
IF OBJECT_ID(N'dbo.AiPrompts', N'U') IS NOT NULL DROP TABLE dbo.AiPrompts;
IF OBJECT_ID(N'dbo.MediaFiles', N'U') IS NOT NULL DROP TABLE dbo.MediaFiles;
IF OBJECT_ID(N'dbo.ChannelAccounts', N'U') IS NOT NULL DROP TABLE dbo.ChannelAccounts;
IF OBJECT_ID(N'dbo.CreditTransactions', N'U') IS NOT NULL DROP TABLE dbo.CreditTransactions;
IF OBJECT_ID(N'dbo.Credits', N'U') IS NOT NULL DROP TABLE dbo.Credits;
IF OBJECT_ID(N'dbo.Subscriptions', N'U') IS NOT NULL DROP TABLE dbo.Subscriptions;
IF OBJECT_ID(N'dbo.Projects', N'U') IS NOT NULL DROP TABLE dbo.Projects;
IF OBJECT_ID(N'dbo.UserRoles', N'U') IS NOT NULL DROP TABLE dbo.UserRoles;
IF OBJECT_ID(N'dbo.RolePermissions', N'U') IS NOT NULL DROP TABLE dbo.RolePermissions;
IF OBJECT_ID(N'dbo.Users', N'U') IS NOT NULL DROP TABLE dbo.Users;
IF OBJECT_ID(N'dbo.Roles', N'U') IS NOT NULL DROP TABLE dbo.Roles;
IF OBJECT_ID(N'dbo.Permissions', N'U') IS NOT NULL DROP TABLE dbo.Permissions;
IF OBJECT_ID(N'dbo.Plans', N'U') IS NOT NULL DROP TABLE dbo.Plans;
IF OBJECT_ID(N'dbo.Channels', N'U') IS NOT NULL DROP TABLE dbo.Channels;
IF OBJECT_ID(N'dbo.Settings', N'U') IS NOT NULL DROP TABLE dbo.Settings;
GO

-- =============================================
-- Core Tables
-- =============================================

CREATE TABLE dbo.Users (
    Id              UNIQUEIDENTIFIER NOT NULL,
    Email           NVARCHAR(256)    NOT NULL,
    PasswordHash    NVARCHAR(512)    NOT NULL,
    FirstName       NVARCHAR(100)    NOT NULL,
    LastName        NVARCHAR(100)    NOT NULL,
    AvatarUrl       NVARCHAR(500)    NULL,
    Phone           NVARCHAR(20)     NULL,
    IsActive        BIT              NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT (1),
    LastLoginAt     DATETIME2(7)     NULL,
    GoogleId        NVARCHAR(128)    NULL,
    FacebookId      NVARCHAR(128)    NULL,
    ReferralCode    NVARCHAR(32)     NULL,
    ReferredByUserId UNIQUEIDENTIFIER NULL,
    CreatedAt       DATETIME2(7)     NOT NULL,
    UpdatedAt       DATETIME2(7)     NULL,
    CreatedBy       UNIQUEIDENTIFIER NULL,
    UpdatedBy       UNIQUEIDENTIFIER NULL,
    IsDeleted       BIT              NOT NULL CONSTRAINT DF_Users_IsDeleted DEFAULT (0),
    CONSTRAINT PK_Users PRIMARY KEY CLUSTERED (Id)
);
GO

CREATE TABLE dbo.Roles (
    Id          UNIQUEIDENTIFIER NOT NULL,
    Name        NVARCHAR(100)    NOT NULL,
    Description NVARCHAR(500)    NULL,
    IsSystem    BIT              NOT NULL CONSTRAINT DF_Roles_IsSystem DEFAULT (0),
    CreatedAt   DATETIME2(7)     NOT NULL,
    UpdatedAt   DATETIME2(7)     NULL,
    CreatedBy   UNIQUEIDENTIFIER NULL,
    UpdatedBy   UNIQUEIDENTIFIER NULL,
    IsDeleted   BIT              NOT NULL CONSTRAINT DF_Roles_IsDeleted DEFAULT (0),
    CONSTRAINT PK_Roles PRIMARY KEY CLUSTERED (Id)
);
GO

CREATE TABLE dbo.Permissions (
    Id          UNIQUEIDENTIFIER NOT NULL,
    Name        NVARCHAR(200)    NOT NULL,
    Code        NVARCHAR(100)    NOT NULL,
    Description NVARCHAR(500)    NULL,
    Module      NVARCHAR(100)    NULL,
    CreatedAt   DATETIME2(7)     NOT NULL,
    UpdatedAt   DATETIME2(7)     NULL,
    CreatedBy   UNIQUEIDENTIFIER NULL,
    UpdatedBy   UNIQUEIDENTIFIER NULL,
    IsDeleted   BIT              NOT NULL CONSTRAINT DF_Permissions_IsDeleted DEFAULT (0),
    CONSTRAINT PK_Permissions PRIMARY KEY CLUSTERED (Id)
);
GO

CREATE TABLE dbo.RolePermissions (
    Id           UNIQUEIDENTIFIER NOT NULL,
    RoleId       UNIQUEIDENTIFIER NOT NULL,
    PermissionId UNIQUEIDENTIFIER NOT NULL,
    CreatedAt    DATETIME2(7)     NOT NULL,
    UpdatedAt    DATETIME2(7)     NULL,
    CreatedBy    UNIQUEIDENTIFIER NULL,
    UpdatedBy    UNIQUEIDENTIFIER NULL,
    IsDeleted    BIT              NOT NULL CONSTRAINT DF_RolePermissions_IsDeleted DEFAULT (0),
    CONSTRAINT PK_RolePermissions PRIMARY KEY CLUSTERED (Id)
);
GO

CREATE TABLE dbo.Plans (
    Id                UNIQUEIDENTIFIER NOT NULL,
    Name              NVARCHAR(100)    NOT NULL,
    Code              NVARCHAR(50)     NOT NULL,
    Description       NVARCHAR(1000)   NULL,
    Price             DECIMAL(18,2)    NOT NULL,
    BillingPeriod     INT              NOT NULL,
    MaxProjects       INT              NOT NULL,
    MaxChannels       INT              NOT NULL,
    MaxPostsPerMonth  INT              NOT NULL,
    CreditsIncluded   INT              NOT NULL,
    IsActive          BIT              NOT NULL CONSTRAINT DF_Plans_IsActive DEFAULT (1),
    SortOrder         INT              NOT NULL,
    CreatedAt         DATETIME2(7)     NOT NULL,
    UpdatedAt         DATETIME2(7)     NULL,
    CreatedBy         UNIQUEIDENTIFIER NULL,
    UpdatedBy         UNIQUEIDENTIFIER NULL,
    IsDeleted         BIT              NOT NULL CONSTRAINT DF_Plans_IsDeleted DEFAULT (0),
    CONSTRAINT PK_Plans PRIMARY KEY CLUSTERED (Id)
);
GO

CREATE TABLE dbo.Channels (
    Id          UNIQUEIDENTIFIER NOT NULL,
    Name        NVARCHAR(100)    NOT NULL,
    Code        NVARCHAR(50)     NOT NULL,
    Description NVARCHAR(500)    NULL,
    IconUrl     NVARCHAR(500)    NULL,
    IsActive    BIT              NOT NULL CONSTRAINT DF_Channels_IsActive DEFAULT (1),
    SortOrder   INT              NOT NULL,
    CreatedAt   DATETIME2(7)     NOT NULL,
    UpdatedAt   DATETIME2(7)     NULL,
    CreatedBy   UNIQUEIDENTIFIER NULL,
    UpdatedBy   UNIQUEIDENTIFIER NULL,
    IsDeleted   BIT              NOT NULL CONSTRAINT DF_Channels_IsDeleted DEFAULT (0),
    CONSTRAINT PK_Channels PRIMARY KEY CLUSTERED (Id)
);
GO

CREATE TABLE dbo.Settings (
    Id          UNIQUEIDENTIFIER NOT NULL,
    [Key]       NVARCHAR(200)    NOT NULL,
    Value       NVARCHAR(MAX)    NOT NULL,
    Category    NVARCHAR(100)    NULL,
    Description NVARCHAR(500)    NULL,
    IsPublic    BIT              NOT NULL CONSTRAINT DF_Settings_IsPublic DEFAULT (0),
    CreatedAt   DATETIME2(7)     NOT NULL,
    UpdatedAt   DATETIME2(7)     NULL,
    CreatedBy   UNIQUEIDENTIFIER NULL,
    UpdatedBy   UNIQUEIDENTIFIER NULL,
    IsDeleted   BIT              NOT NULL CONSTRAINT DF_Settings_IsDeleted DEFAULT (0),
    CONSTRAINT PK_Settings PRIMARY KEY CLUSTERED (Id)
);
GO

CREATE TABLE dbo.UserRoles (
    Id        UNIQUEIDENTIFIER NOT NULL,
    UserId    UNIQUEIDENTIFIER NOT NULL,
    RoleId    UNIQUEIDENTIFIER NOT NULL,
    CreatedAt DATETIME2(7)     NOT NULL,
    UpdatedAt DATETIME2(7)     NULL,
    CreatedBy UNIQUEIDENTIFIER NULL,
    UpdatedBy UNIQUEIDENTIFIER NULL,
    IsDeleted BIT              NOT NULL CONSTRAINT DF_UserRoles_IsDeleted DEFAULT (0),
    CONSTRAINT PK_UserRoles PRIMARY KEY CLUSTERED (Id)
);
GO

CREATE TABLE dbo.Subscriptions (
    Id          UNIQUEIDENTIFIER NOT NULL,
    UserId      UNIQUEIDENTIFIER NOT NULL,
    PlanId      UNIQUEIDENTIFIER NOT NULL,
    Status      INT              NOT NULL,
    StartDate   DATETIME2(7)     NOT NULL,
    EndDate     DATETIME2(7)     NULL,
    CancelledAt DATETIME2(7)     NULL,
    AutoRenew   BIT              NOT NULL CONSTRAINT DF_Subscriptions_AutoRenew DEFAULT (1),
    CreatedAt   DATETIME2(7)     NOT NULL,
    UpdatedAt   DATETIME2(7)     NULL,
    CreatedBy   UNIQUEIDENTIFIER NULL,
    UpdatedBy   UNIQUEIDENTIFIER NULL,
    IsDeleted   BIT              NOT NULL CONSTRAINT DF_Subscriptions_IsDeleted DEFAULT (0),
    CONSTRAINT PK_Subscriptions PRIMARY KEY CLUSTERED (Id)
);
GO

CREATE TABLE dbo.Credits (
    Id          UNIQUEIDENTIFIER NOT NULL,
    UserId      UNIQUEIDENTIFIER NOT NULL,
    Balance     INT              NOT NULL,
    TotalEarned INT              NOT NULL,
    TotalUsed   INT              NOT NULL,
    CreatedAt   DATETIME2(7)     NOT NULL,
    UpdatedAt   DATETIME2(7)     NULL,
    CreatedBy   UNIQUEIDENTIFIER NULL,
    UpdatedBy   UNIQUEIDENTIFIER NULL,
    IsDeleted   BIT              NOT NULL CONSTRAINT DF_Credits_IsDeleted DEFAULT (0),
    CONSTRAINT PK_Credits PRIMARY KEY CLUSTERED (Id)
);
GO

CREATE TABLE dbo.Projects (
    Id          UNIQUEIDENTIFIER NOT NULL,
    UserId      UNIQUEIDENTIFIER NOT NULL,
    Name        NVARCHAR(200)    NOT NULL,
    Description NVARCHAR(2000)   NULL,
    LogoUrl     NVARCHAR(500)    NULL,
    IsActive    BIT              NOT NULL CONSTRAINT DF_Projects_IsActive DEFAULT (1),
    CreatedAt   DATETIME2(7)     NOT NULL,
    UpdatedAt   DATETIME2(7)     NULL,
    CreatedBy   UNIQUEIDENTIFIER NULL,
    UpdatedBy   UNIQUEIDENTIFIER NULL,
    IsDeleted   BIT              NOT NULL CONSTRAINT DF_Projects_IsDeleted DEFAULT (0),
    CONSTRAINT PK_Projects PRIMARY KEY CLUSTERED (Id)
);
GO

CREATE TABLE dbo.CreditTransactions (
    Id            UNIQUEIDENTIFIER NOT NULL,
    CreditId      UNIQUEIDENTIFIER NOT NULL,
    UserId        UNIQUEIDENTIFIER NOT NULL,
    Type          INT              NOT NULL,
    Amount        INT              NOT NULL,
    BalanceAfter  INT              NOT NULL,
    Description   NVARCHAR(500)    NULL,
    ReferenceType NVARCHAR(100)    NULL,
    ReferenceId   UNIQUEIDENTIFIER NULL,
    CreatedAt     DATETIME2(7)     NOT NULL,
    UpdatedAt     DATETIME2(7)     NULL,
    CreatedBy     UNIQUEIDENTIFIER NULL,
    UpdatedBy     UNIQUEIDENTIFIER NULL,
    IsDeleted     BIT              NOT NULL CONSTRAINT DF_CreditTransactions_IsDeleted DEFAULT (0),
    CONSTRAINT PK_CreditTransactions PRIMARY KEY CLUSTERED (Id)
);
GO

CREATE TABLE dbo.ChannelAccounts (
    Id           UNIQUEIDENTIFIER NOT NULL,
    ProjectId    UNIQUEIDENTIFIER NOT NULL,
    ChannelId    UNIQUEIDENTIFIER NOT NULL,
    UserId       UNIQUEIDENTIFIER NOT NULL,
    Name         NVARCHAR(200)    NOT NULL,
    ExternalId   NVARCHAR(128)    NULL,
    ProfileUrl   NVARCHAR(500)    NULL,
    AvatarUrl    NVARCHAR(500)    NULL,
    IsActive     BIT              NOT NULL CONSTRAINT DF_ChannelAccounts_IsActive DEFAULT (1),
    LastSyncedAt DATETIME2(7)     NULL,
    CreatedAt    DATETIME2(7)     NOT NULL,
    UpdatedAt    DATETIME2(7)     NULL,
    CreatedBy    UNIQUEIDENTIFIER NULL,
    UpdatedBy    UNIQUEIDENTIFIER NULL,
    IsDeleted    BIT              NOT NULL CONSTRAINT DF_ChannelAccounts_IsDeleted DEFAULT (0),
    CONSTRAINT PK_ChannelAccounts PRIMARY KEY CLUSTERED (Id)
);
GO

CREATE TABLE dbo.MediaFiles (
    Id           UNIQUEIDENTIFIER NOT NULL,
    UserId       UNIQUEIDENTIFIER NOT NULL,
    ProjectId    UNIQUEIDENTIFIER NULL,
    FileName     NVARCHAR(255)    NOT NULL,
    FileUrl      NVARCHAR(1000)   NOT NULL,
    MimeType     NVARCHAR(100)    NOT NULL,
    FileSize     BIGINT           NOT NULL,
    ThumbnailUrl NVARCHAR(1000)   NULL,
    Width        INT              NULL,
    Height       INT              NULL,
    CreatedAt    DATETIME2(7)     NOT NULL,
    UpdatedAt    DATETIME2(7)     NULL,
    CreatedBy    UNIQUEIDENTIFIER NULL,
    UpdatedBy    UNIQUEIDENTIFIER NULL,
    IsDeleted    BIT              NOT NULL CONSTRAINT DF_MediaFiles_IsDeleted DEFAULT (0),
    CONSTRAINT PK_MediaFiles PRIMARY KEY CLUSTERED (Id)
);
GO

CREATE TABLE dbo.AiPrompts (
    Id        UNIQUEIDENTIFIER NOT NULL,
    UserId    UNIQUEIDENTIFIER NULL,
    ProjectId UNIQUEIDENTIFIER NULL,
    Name      NVARCHAR(200)    NOT NULL,
    Template  NVARCHAR(MAX)    NOT NULL,
    Category  NVARCHAR(100)    NULL,
    IsSystem  BIT              NOT NULL CONSTRAINT DF_AiPrompts_IsSystem DEFAULT (0),
    CreatedAt DATETIME2(7)     NOT NULL,
    UpdatedAt DATETIME2(7)     NULL,
    CreatedBy UNIQUEIDENTIFIER NULL,
    UpdatedBy UNIQUEIDENTIFIER NULL,
    IsDeleted BIT              NOT NULL CONSTRAINT DF_AiPrompts_IsDeleted DEFAULT (0),
    CONSTRAINT PK_AiPrompts PRIMARY KEY CLUSTERED (Id)
);
GO

CREATE TABLE dbo.AiGeneratedContents (
    Id          UNIQUEIDENTIFIER NOT NULL,
    AiPromptId  UNIQUEIDENTIFIER NOT NULL,
    UserId      UNIQUEIDENTIFIER NOT NULL,
    ProjectId   UNIQUEIDENTIFIER NULL,
    Input       NVARCHAR(MAX)    NOT NULL,
    Output      NVARCHAR(MAX)    NULL,
    TokensUsed  INT              NOT NULL,
    Status      INT              NOT NULL,
    ErrorMessage NVARCHAR(2000)  NULL,
    CreatedAt   DATETIME2(7)     NOT NULL,
    UpdatedAt   DATETIME2(7)     NULL,
    CreatedBy   UNIQUEIDENTIFIER NULL,
    UpdatedBy   UNIQUEIDENTIFIER NULL,
    IsDeleted   BIT              NOT NULL CONSTRAINT DF_AiGeneratedContents_IsDeleted DEFAULT (0),
    CONSTRAINT PK_AiGeneratedContents PRIMARY KEY CLUSTERED (Id)
);
GO

CREATE TABLE dbo.Posts (
    Id                UNIQUEIDENTIFIER NOT NULL,
    ProjectId         UNIQUEIDENTIFIER NOT NULL,
    ChannelAccountId  UNIQUEIDENTIFIER NOT NULL,
    Title             NVARCHAR(500)    NOT NULL,
    Status            INT              NOT NULL,
    ExternalPostId    NVARCHAR(128)    NULL,
    PublishedUrl      NVARCHAR(1000)   NULL,
    PublishedAt       DATETIME2(7)     NULL,
    CreatedAt         DATETIME2(7)     NOT NULL,
    UpdatedAt         DATETIME2(7)     NULL,
    CreatedBy         UNIQUEIDENTIFIER NULL,
    UpdatedBy         UNIQUEIDENTIFIER NULL,
    IsDeleted         BIT              NOT NULL CONSTRAINT DF_Posts_IsDeleted DEFAULT (0),
    CONSTRAINT PK_Posts PRIMARY KEY CLUSTERED (Id)
);
GO

CREATE TABLE dbo.PostContents (
    Id          UNIQUEIDENTIFIER NOT NULL,
    PostId      UNIQUEIDENTIFIER NOT NULL,
    ContentType INT              NOT NULL,
    Content     NVARCHAR(MAX)    NOT NULL,
    SortOrder   INT              NOT NULL,
    MediaFileId UNIQUEIDENTIFIER NULL,
    CreatedAt   DATETIME2(7)     NOT NULL,
    UpdatedAt   DATETIME2(7)     NULL,
    CreatedBy   UNIQUEIDENTIFIER NULL,
    UpdatedBy   UNIQUEIDENTIFIER NULL,
    IsDeleted   BIT              NOT NULL CONSTRAINT DF_PostContents_IsDeleted DEFAULT (0),
    CONSTRAINT PK_PostContents PRIMARY KEY CLUSTERED (Id)
);
GO

CREATE TABLE dbo.PostSchedules (
    Id            UNIQUEIDENTIFIER NOT NULL,
    PostId        UNIQUEIDENTIFIER NOT NULL,
    ScheduledAt   DATETIME2(7)     NOT NULL,
    Status        INT              NOT NULL,
    ExecutedAt    DATETIME2(7)     NULL,
    FailureReason NVARCHAR(1000)   NULL,
    RetryCount    INT              NOT NULL CONSTRAINT DF_PostSchedules_RetryCount DEFAULT (0),
    CreatedAt     DATETIME2(7)     NOT NULL,
    UpdatedAt     DATETIME2(7)     NULL,
    CreatedBy     UNIQUEIDENTIFIER NULL,
    UpdatedBy     UNIQUEIDENTIFIER NULL,
    IsDeleted     BIT              NOT NULL CONSTRAINT DF_PostSchedules_IsDeleted DEFAULT (0),
    CONSTRAINT PK_PostSchedules PRIMARY KEY CLUSTERED (Id)
);
GO

CREATE TABLE dbo.PostLogs (
    Id        UNIQUEIDENTIFIER NOT NULL,
    PostId    UNIQUEIDENTIFIER NOT NULL,
    Action    NVARCHAR(100)    NOT NULL,
    Message   NVARCHAR(2000)   NOT NULL,
    Level     INT              NOT NULL,
    Details   NVARCHAR(MAX)    NULL,
    CreatedAt DATETIME2(7)     NOT NULL,
    UpdatedAt DATETIME2(7)     NULL,
    CreatedBy UNIQUEIDENTIFIER NULL,
    UpdatedBy UNIQUEIDENTIFIER NULL,
    IsDeleted BIT              NOT NULL CONSTRAINT DF_PostLogs_IsDeleted DEFAULT (0),
    CONSTRAINT PK_PostLogs PRIMARY KEY CLUSTERED (Id)
);
GO

CREATE TABLE dbo.FacebookAccounts (
    Id                 UNIQUEIDENTIFIER NOT NULL,
    UserId             UNIQUEIDENTIFIER NOT NULL,
    FacebookUserId     NVARCHAR(128)    NOT NULL,
    Name               NVARCHAR(200)    NOT NULL,
    Email              NVARCHAR(256)    NULL,
    ProfilePictureUrl  NVARCHAR(500)    NULL,
    IsActive           BIT              NOT NULL CONSTRAINT DF_FacebookAccounts_IsActive DEFAULT (1),
    TokenExpiresAt     DATETIME2(7)     NULL,
    LastSyncedAt       DATETIME2(7)     NULL,
    CreatedAt          DATETIME2(7)     NOT NULL,
    UpdatedAt          DATETIME2(7)     NULL,
    CreatedBy          UNIQUEIDENTIFIER NULL,
    UpdatedBy          UNIQUEIDENTIFIER NULL,
    IsDeleted          BIT              NOT NULL CONSTRAINT DF_FacebookAccounts_IsDeleted DEFAULT (0),
    CONSTRAINT PK_FacebookAccounts PRIMARY KEY CLUSTERED (Id)
);
GO

CREATE TABLE dbo.FacebookPages (
    Id                UNIQUEIDENTIFIER NOT NULL,
    FacebookAccountId UNIQUEIDENTIFIER NOT NULL,
    PageId            NVARCHAR(128)    NOT NULL,
    Name              NVARCHAR(200)    NOT NULL,
    Category          NVARCHAR(100)    NULL,
    ProfilePictureUrl NVARCHAR(500)    NULL,
    IsConnected       BIT              NOT NULL CONSTRAINT DF_FacebookPages_IsConnected DEFAULT (1),
    LastSyncedAt      DATETIME2(7)     NULL,
    CreatedAt         DATETIME2(7)     NOT NULL,
    UpdatedAt         DATETIME2(7)     NULL,
    CreatedBy         UNIQUEIDENTIFIER NULL,
    UpdatedBy         UNIQUEIDENTIFIER NULL,
    IsDeleted         BIT              NOT NULL CONSTRAINT DF_FacebookPages_IsDeleted DEFAULT (0),
    CONSTRAINT PK_FacebookPages PRIMARY KEY CLUSTERED (Id)
);
GO

CREATE TABLE dbo.WordPressSites (
    Id           UNIQUEIDENTIFIER NOT NULL,
    UserId       UNIQUEIDENTIFIER NOT NULL,
    SiteUrl      NVARCHAR(500)    NOT NULL,
    SiteName     NVARCHAR(200)    NOT NULL,
    Username     NVARCHAR(100)    NOT NULL,
    IsConnected  BIT              NOT NULL CONSTRAINT DF_WordPressSites_IsConnected DEFAULT (1),
    LastSyncedAt DATETIME2(7)     NULL,
    CreatedAt    DATETIME2(7)     NOT NULL,
    UpdatedAt    DATETIME2(7)     NULL,
    CreatedBy    UNIQUEIDENTIFIER NULL,
    UpdatedBy    UNIQUEIDENTIFIER NULL,
    IsDeleted    BIT              NOT NULL CONSTRAINT DF_WordPressSites_IsDeleted DEFAULT (0),
    CONSTRAINT PK_WordPressSites PRIMARY KEY CLUSTERED (Id)
);
GO

CREATE TABLE dbo.WordPressPosts (
    Id              UNIQUEIDENTIFIER NOT NULL,
    WordPressSiteId UNIQUEIDENTIFIER NOT NULL,
    ExternalPostId  NVARCHAR(128)    NOT NULL,
    Title           NVARCHAR(500)    NOT NULL,
    Excerpt         NVARCHAR(2000)   NULL,
    Status          INT              NOT NULL,
    Permalink       NVARCHAR(1000)   NULL,
    PublishedAt     DATETIME2(7)     NULL,
    CreatedAt       DATETIME2(7)     NOT NULL,
    UpdatedAt       DATETIME2(7)     NULL,
    CreatedBy       UNIQUEIDENTIFIER NULL,
    UpdatedBy       UNIQUEIDENTIFIER NULL,
    IsDeleted       BIT              NOT NULL CONSTRAINT DF_WordPressPosts_IsDeleted DEFAULT (0),
    CONSTRAINT PK_WordPressPosts PRIMARY KEY CLUSTERED (Id)
);
GO

CREATE TABLE dbo.ZaloAccounts (
    Id           UNIQUEIDENTIFIER NOT NULL,
    UserId       UNIQUEIDENTIFIER NOT NULL,
    ZaloUserId   NVARCHAR(128)    NOT NULL,
    DisplayName  NVARCHAR(200)    NOT NULL,
    AvatarUrl    NVARCHAR(500)    NULL,
    IsConnected  BIT              NOT NULL CONSTRAINT DF_ZaloAccounts_IsConnected DEFAULT (1),
    TokenExpiresAt DATETIME2(7)   NULL,
    LastSyncedAt DATETIME2(7)     NULL,
    CreatedAt    DATETIME2(7)     NOT NULL,
    UpdatedAt    DATETIME2(7)     NULL,
    CreatedBy    UNIQUEIDENTIFIER NULL,
    UpdatedBy    UNIQUEIDENTIFIER NULL,
    IsDeleted    BIT              NOT NULL CONSTRAINT DF_ZaloAccounts_IsDeleted DEFAULT (0),
    CONSTRAINT PK_ZaloAccounts PRIMARY KEY CLUSTERED (Id)
);
GO

CREATE TABLE dbo.ZaloPosts (
    Id             UNIQUEIDENTIFIER NOT NULL,
    ZaloAccountId  UNIQUEIDENTIFIER NOT NULL,
    ExternalPostId NVARCHAR(128)    NULL,
    Content        NVARCHAR(MAX)    NOT NULL,
    Status         INT              NOT NULL,
    PublishedAt    DATETIME2(7)     NULL,
    CreatedAt      DATETIME2(7)     NOT NULL,
    UpdatedAt      DATETIME2(7)     NULL,
    CreatedBy      UNIQUEIDENTIFIER NULL,
    UpdatedBy      UNIQUEIDENTIFIER NULL,
    IsDeleted      BIT              NOT NULL CONSTRAINT DF_ZaloPosts_IsDeleted DEFAULT (0),
    CONSTRAINT PK_ZaloPosts PRIMARY KEY CLUSTERED (Id)
);
GO

CREATE TABLE dbo.Notifications (
    Id            UNIQUEIDENTIFIER NOT NULL,
    UserId        UNIQUEIDENTIFIER NOT NULL,
    Title         NVARCHAR(200)    NOT NULL,
    Message       NVARCHAR(2000)   NOT NULL,
    Type          INT              NOT NULL,
    IsRead        BIT              NOT NULL CONSTRAINT DF_Notifications_IsRead DEFAULT (0),
    ReadAt        DATETIME2(7)     NULL,
    ReferenceType NVARCHAR(100)    NULL,
    ReferenceId   UNIQUEIDENTIFIER NULL,
    CreatedAt     DATETIME2(7)     NOT NULL,
    UpdatedAt     DATETIME2(7)     NULL,
    CreatedBy     UNIQUEIDENTIFIER NULL,
    UpdatedBy     UNIQUEIDENTIFIER NULL,
    IsDeleted     BIT              NOT NULL CONSTRAINT DF_Notifications_IsDeleted DEFAULT (0),
    CONSTRAINT PK_Notifications PRIMARY KEY CLUSTERED (Id)
);
GO

CREATE TABLE dbo.Affiliates (
    Id             UNIQUEIDENTIFIER NOT NULL,
    UserId         UNIQUEIDENTIFIER NOT NULL,
    Code           NVARCHAR(32)     NOT NULL,
    CommissionRate DECIMAL(5,4)   NOT NULL,
    TotalEarnings  DECIMAL(18,2)  NOT NULL CONSTRAINT DF_Affiliates_TotalEarnings DEFAULT (0),
    IsActive       BIT            NOT NULL CONSTRAINT DF_Affiliates_IsActive DEFAULT (1),
    CreatedAt      DATETIME2(7)   NOT NULL,
    UpdatedAt      DATETIME2(7)   NULL,
    CreatedBy      UNIQUEIDENTIFIER NULL,
    UpdatedBy      UNIQUEIDENTIFIER NULL,
    IsDeleted      BIT            NOT NULL CONSTRAINT DF_Affiliates_IsDeleted DEFAULT (0),
    CONSTRAINT PK_Affiliates PRIMARY KEY CLUSTERED (Id)
);
GO

CREATE TABLE dbo.AffiliateLinks (
    Id          UNIQUEIDENTIFIER NOT NULL,
    AffiliateId UNIQUEIDENTIFIER NOT NULL,
    Url         NVARCHAR(1000)   NOT NULL,
    Campaign    NVARCHAR(100)    NULL,
    ClickCount  INT              NOT NULL CONSTRAINT DF_AffiliateLinks_ClickCount DEFAULT (0),
    CreatedAt   DATETIME2(7)     NOT NULL,
    UpdatedAt   DATETIME2(7)     NULL,
    CreatedBy   UNIQUEIDENTIFIER NULL,
    UpdatedBy   UNIQUEIDENTIFIER NULL,
    IsDeleted   BIT              NOT NULL CONSTRAINT DF_AffiliateLinks_IsDeleted DEFAULT (0),
    CONSTRAINT PK_AffiliateLinks PRIMARY KEY CLUSTERED (Id)
);
GO

CREATE TABLE dbo.Invoices (
    Id             UNIQUEIDENTIFIER NOT NULL,
    UserId         UNIQUEIDENTIFIER NOT NULL,
    SubscriptionId UNIQUEIDENTIFIER NULL,
    InvoiceNumber  NVARCHAR(50)     NOT NULL,
    Amount         DECIMAL(18,2)    NOT NULL,
    Tax            DECIMAL(18,2)    NOT NULL,
    Total          DECIMAL(18,2)    NOT NULL,
    Status         INT              NOT NULL,
    DueDate        DATETIME2(7)     NOT NULL,
    PaidAt         DATETIME2(7)     NULL,
    Notes          NVARCHAR(1000)   NULL,
    CreatedAt      DATETIME2(7)     NOT NULL,
    UpdatedAt      DATETIME2(7)     NULL,
    CreatedBy      UNIQUEIDENTIFIER NULL,
    UpdatedBy      UNIQUEIDENTIFIER NULL,
    IsDeleted      BIT              NOT NULL CONSTRAINT DF_Invoices_IsDeleted DEFAULT (0),
    CONSTRAINT PK_Invoices PRIMARY KEY CLUSTERED (Id)
);
GO

CREATE TABLE dbo.Payments (
    Id            UNIQUEIDENTIFIER NOT NULL,
    InvoiceId     UNIQUEIDENTIFIER NOT NULL,
    UserId        UNIQUEIDENTIFIER NOT NULL,
    Amount        DECIMAL(18,2)    NOT NULL,
    PaymentMethod INT              NOT NULL,
    TransactionId NVARCHAR(128)    NULL,
    Status        INT              NOT NULL,
    PaidAt        DATETIME2(7)     NULL,
    FailureReason NVARCHAR(1000)   NULL,
    CreatedAt     DATETIME2(7)     NOT NULL,
    UpdatedAt     DATETIME2(7)     NULL,
    CreatedBy     UNIQUEIDENTIFIER NULL,
    UpdatedBy     UNIQUEIDENTIFIER NULL,
    IsDeleted     BIT              NOT NULL CONSTRAINT DF_Payments_IsDeleted DEFAULT (0),
    CONSTRAINT PK_Payments PRIMARY KEY CLUSTERED (Id)
);
GO

CREATE TABLE dbo.AffiliateCommissions (
    Id             UNIQUEIDENTIFIER NOT NULL,
    AffiliateId    UNIQUEIDENTIFIER NOT NULL,
    ReferredUserId UNIQUEIDENTIFIER NOT NULL,
    SubscriptionId UNIQUEIDENTIFIER NULL,
    Amount         DECIMAL(18,2)    NOT NULL,
    Status         INT              NOT NULL,
    PaidAt         DATETIME2(7)     NULL,
    CreatedAt      DATETIME2(7)     NOT NULL,
    UpdatedAt      DATETIME2(7)     NULL,
    CreatedBy      UNIQUEIDENTIFIER NULL,
    UpdatedBy      UNIQUEIDENTIFIER NULL,
    IsDeleted      BIT              NOT NULL CONSTRAINT DF_AffiliateCommissions_IsDeleted DEFAULT (0),
    CONSTRAINT PK_AffiliateCommissions PRIMARY KEY CLUSTERED (Id)
);
GO

CREATE TABLE dbo.AuditLogs (
    Id         UNIQUEIDENTIFIER NOT NULL,
    UserId     UNIQUEIDENTIFIER NULL,
    Action     NVARCHAR(100)    NOT NULL,
    EntityType NVARCHAR(200)    NOT NULL,
    EntityId   UNIQUEIDENTIFIER NOT NULL,
    OldValues  NVARCHAR(MAX)    NULL,
    NewValues  NVARCHAR(MAX)    NULL,
    IpAddress  NVARCHAR(45)     NULL,
    UserAgent  NVARCHAR(500)    NULL,
    CreatedAt  DATETIME2(7)     NOT NULL,
    UpdatedAt  DATETIME2(7)     NULL,
    CreatedBy  UNIQUEIDENTIFIER NULL,
    UpdatedBy  UNIQUEIDENTIFIER NULL,
    IsDeleted  BIT              NOT NULL CONSTRAINT DF_AuditLogs_IsDeleted DEFAULT (0),
    CONSTRAINT PK_AuditLogs PRIMARY KEY CLUSTERED (Id)
);
GO

CREATE TABLE dbo.RefreshTokens (
    Id              UNIQUEIDENTIFIER NOT NULL,
    UserId          UNIQUEIDENTIFIER NOT NULL,
    Token           NVARCHAR(512)    NOT NULL,
    ExpiresAt       DATETIME2(7)     NOT NULL,
    RevokedAt       DATETIME2(7)     NULL,
    ReplacedByToken NVARCHAR(512)    NULL,
    CreatedAt       DATETIME2(7)     NOT NULL,
    UpdatedAt       DATETIME2(7)     NULL,
    CreatedBy       UNIQUEIDENTIFIER NULL,
    UpdatedBy       UNIQUEIDENTIFIER NULL,
    IsDeleted       BIT              NOT NULL CONSTRAINT DF_RefreshTokens_IsDeleted DEFAULT (0),
    CONSTRAINT PK_RefreshTokens PRIMARY KEY CLUSTERED (Id)
);
GO

CREATE TABLE dbo.PasswordResetTokens (
    Id        UNIQUEIDENTIFIER NOT NULL,
    UserId    UNIQUEIDENTIFIER NOT NULL,
    Token     NVARCHAR(512)    NOT NULL,
    ExpiresAt DATETIME2(7)     NOT NULL,
    UsedAt    DATETIME2(7)     NULL,
    CreatedAt DATETIME2(7)     NOT NULL,
    UpdatedAt DATETIME2(7)     NULL,
    CreatedBy UNIQUEIDENTIFIER NULL,
    UpdatedBy UNIQUEIDENTIFIER NULL,
    IsDeleted BIT              NOT NULL CONSTRAINT DF_PasswordResetTokens_IsDeleted DEFAULT (0),
    CONSTRAINT PK_PasswordResetTokens PRIMARY KEY CLUSTERED (Id)
);
GO

-- =============================================
-- Foreign Keys
-- =============================================

ALTER TABLE dbo.Users ADD CONSTRAINT FK_Users_ReferredByUser
    FOREIGN KEY (ReferredByUserId) REFERENCES dbo.Users(Id);

ALTER TABLE dbo.UserRoles ADD CONSTRAINT FK_UserRoles_Users
    FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE CASCADE;
ALTER TABLE dbo.UserRoles ADD CONSTRAINT FK_UserRoles_Roles
    FOREIGN KEY (RoleId) REFERENCES dbo.Roles(Id) ON DELETE CASCADE;

ALTER TABLE dbo.RolePermissions ADD CONSTRAINT FK_RolePermissions_Roles
    FOREIGN KEY (RoleId) REFERENCES dbo.Roles(Id) ON DELETE CASCADE;
ALTER TABLE dbo.RolePermissions ADD CONSTRAINT FK_RolePermissions_Permissions
    FOREIGN KEY (PermissionId) REFERENCES dbo.Permissions(Id) ON DELETE CASCADE;

ALTER TABLE dbo.Subscriptions ADD CONSTRAINT FK_Subscriptions_Users
    FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE CASCADE;
ALTER TABLE dbo.Subscriptions ADD CONSTRAINT FK_Subscriptions_Plans
    FOREIGN KEY (PlanId) REFERENCES dbo.Plans(Id);

ALTER TABLE dbo.Credits ADD CONSTRAINT FK_Credits_Users
    FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE CASCADE;

ALTER TABLE dbo.Projects ADD CONSTRAINT FK_Projects_Users
    FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE CASCADE;

ALTER TABLE dbo.CreditTransactions ADD CONSTRAINT FK_CreditTransactions_Credits
    FOREIGN KEY (CreditId) REFERENCES dbo.Credits(Id) ON DELETE CASCADE;
ALTER TABLE dbo.CreditTransactions ADD CONSTRAINT FK_CreditTransactions_Users
    FOREIGN KEY (UserId) REFERENCES dbo.Users(Id);

ALTER TABLE dbo.ChannelAccounts ADD CONSTRAINT FK_ChannelAccounts_Projects
    FOREIGN KEY (ProjectId) REFERENCES dbo.Projects(Id) ON DELETE CASCADE;
ALTER TABLE dbo.ChannelAccounts ADD CONSTRAINT FK_ChannelAccounts_Channels
    FOREIGN KEY (ChannelId) REFERENCES dbo.Channels(Id);
ALTER TABLE dbo.ChannelAccounts ADD CONSTRAINT FK_ChannelAccounts_Users
    FOREIGN KEY (UserId) REFERENCES dbo.Users(Id);

ALTER TABLE dbo.MediaFiles ADD CONSTRAINT FK_MediaFiles_Users
    FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE CASCADE;
ALTER TABLE dbo.MediaFiles ADD CONSTRAINT FK_MediaFiles_Projects
    FOREIGN KEY (ProjectId) REFERENCES dbo.Projects(Id) ON DELETE SET NULL;

ALTER TABLE dbo.AiPrompts ADD CONSTRAINT FK_AiPrompts_Users
    FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE SET NULL;
ALTER TABLE dbo.AiPrompts ADD CONSTRAINT FK_AiPrompts_Projects
    FOREIGN KEY (ProjectId) REFERENCES dbo.Projects(Id) ON DELETE SET NULL;

ALTER TABLE dbo.AiGeneratedContents ADD CONSTRAINT FK_AiGeneratedContents_AiPrompts
    FOREIGN KEY (AiPromptId) REFERENCES dbo.AiPrompts(Id);
ALTER TABLE dbo.AiGeneratedContents ADD CONSTRAINT FK_AiGeneratedContents_Users
    FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE CASCADE;
ALTER TABLE dbo.AiGeneratedContents ADD CONSTRAINT FK_AiGeneratedContents_Projects
    FOREIGN KEY (ProjectId) REFERENCES dbo.Projects(Id) ON DELETE SET NULL;

ALTER TABLE dbo.Posts ADD CONSTRAINT FK_Posts_Projects
    FOREIGN KEY (ProjectId) REFERENCES dbo.Projects(Id) ON DELETE CASCADE;
ALTER TABLE dbo.Posts ADD CONSTRAINT FK_Posts_ChannelAccounts
    FOREIGN KEY (ChannelAccountId) REFERENCES dbo.ChannelAccounts(Id);

ALTER TABLE dbo.PostContents ADD CONSTRAINT FK_PostContents_Posts
    FOREIGN KEY (PostId) REFERENCES dbo.Posts(Id) ON DELETE CASCADE;
ALTER TABLE dbo.PostContents ADD CONSTRAINT FK_PostContents_MediaFiles
    FOREIGN KEY (MediaFileId) REFERENCES dbo.MediaFiles(Id) ON DELETE SET NULL;

ALTER TABLE dbo.PostSchedules ADD CONSTRAINT FK_PostSchedules_Posts
    FOREIGN KEY (PostId) REFERENCES dbo.Posts(Id) ON DELETE CASCADE;

ALTER TABLE dbo.PostLogs ADD CONSTRAINT FK_PostLogs_Posts
    FOREIGN KEY (PostId) REFERENCES dbo.Posts(Id) ON DELETE CASCADE;

ALTER TABLE dbo.FacebookAccounts ADD CONSTRAINT FK_FacebookAccounts_Users
    FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE CASCADE;

ALTER TABLE dbo.FacebookPages ADD CONSTRAINT FK_FacebookPages_FacebookAccounts
    FOREIGN KEY (FacebookAccountId) REFERENCES dbo.FacebookAccounts(Id) ON DELETE CASCADE;

ALTER TABLE dbo.WordPressSites ADD CONSTRAINT FK_WordPressSites_Users
    FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE CASCADE;

ALTER TABLE dbo.WordPressPosts ADD CONSTRAINT FK_WordPressPosts_WordPressSites
    FOREIGN KEY (WordPressSiteId) REFERENCES dbo.WordPressSites(Id) ON DELETE CASCADE;

ALTER TABLE dbo.ZaloAccounts ADD CONSTRAINT FK_ZaloAccounts_Users
    FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE CASCADE;

ALTER TABLE dbo.ZaloPosts ADD CONSTRAINT FK_ZaloPosts_ZaloAccounts
    FOREIGN KEY (ZaloAccountId) REFERENCES dbo.ZaloAccounts(Id) ON DELETE CASCADE;

ALTER TABLE dbo.Notifications ADD CONSTRAINT FK_Notifications_Users
    FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE CASCADE;

ALTER TABLE dbo.Affiliates ADD CONSTRAINT FK_Affiliates_Users
    FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE CASCADE;

ALTER TABLE dbo.AffiliateLinks ADD CONSTRAINT FK_AffiliateLinks_Affiliates
    FOREIGN KEY (AffiliateId) REFERENCES dbo.Affiliates(Id) ON DELETE CASCADE;

ALTER TABLE dbo.Invoices ADD CONSTRAINT FK_Invoices_Users
    FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE CASCADE;
ALTER TABLE dbo.Invoices ADD CONSTRAINT FK_Invoices_Subscriptions
    FOREIGN KEY (SubscriptionId) REFERENCES dbo.Subscriptions(Id) ON DELETE SET NULL;

ALTER TABLE dbo.Payments ADD CONSTRAINT FK_Payments_Invoices
    FOREIGN KEY (InvoiceId) REFERENCES dbo.Invoices(Id) ON DELETE CASCADE;
ALTER TABLE dbo.Payments ADD CONSTRAINT FK_Payments_Users
    FOREIGN KEY (UserId) REFERENCES dbo.Users(Id);

ALTER TABLE dbo.AffiliateCommissions ADD CONSTRAINT FK_AffiliateCommissions_Affiliates
    FOREIGN KEY (AffiliateId) REFERENCES dbo.Affiliates(Id) ON DELETE CASCADE;
ALTER TABLE dbo.AffiliateCommissions ADD CONSTRAINT FK_AffiliateCommissions_ReferredUsers
    FOREIGN KEY (ReferredUserId) REFERENCES dbo.Users(Id);
ALTER TABLE dbo.AffiliateCommissions ADD CONSTRAINT FK_AffiliateCommissions_Subscriptions
    FOREIGN KEY (SubscriptionId) REFERENCES dbo.Subscriptions(Id) ON DELETE SET NULL;

ALTER TABLE dbo.AuditLogs ADD CONSTRAINT FK_AuditLogs_Users
    FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE SET NULL;

ALTER TABLE dbo.RefreshTokens ADD CONSTRAINT FK_RefreshTokens_Users
    FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE CASCADE;

ALTER TABLE dbo.PasswordResetTokens ADD CONSTRAINT FK_PasswordResetTokens_Users
    FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE CASCADE;
GO

-- =============================================
-- Indexes
-- =============================================

CREATE UNIQUE NONCLUSTERED INDEX IX_Users_Email ON dbo.Users(Email) WHERE IsDeleted = 0;
CREATE NONCLUSTERED INDEX IX_Users_IsDeleted ON dbo.Users(IsDeleted);
CREATE UNIQUE NONCLUSTERED INDEX IX_Users_ReferralCode ON dbo.Users(ReferralCode) WHERE ReferralCode IS NOT NULL AND IsDeleted = 0;

CREATE UNIQUE NONCLUSTERED INDEX IX_Roles_Name ON dbo.Roles(Name) WHERE IsDeleted = 0;
CREATE UNIQUE NONCLUSTERED INDEX IX_Permissions_Code ON dbo.Permissions(Code) WHERE IsDeleted = 0;
CREATE UNIQUE NONCLUSTERED INDEX IX_Plans_Code ON dbo.Plans(Code) WHERE IsDeleted = 0;
CREATE UNIQUE NONCLUSTERED INDEX IX_Plans_Name ON dbo.Plans(Name) WHERE IsDeleted = 0;
CREATE UNIQUE NONCLUSTERED INDEX IX_Channels_Code ON dbo.Channels(Code) WHERE IsDeleted = 0;
CREATE UNIQUE NONCLUSTERED INDEX IX_Settings_Key ON dbo.Settings([Key]) WHERE IsDeleted = 0;

CREATE UNIQUE NONCLUSTERED INDEX IX_UserRoles_UserId_RoleId ON dbo.UserRoles(UserId, RoleId) WHERE IsDeleted = 0;
CREATE UNIQUE NONCLUSTERED INDEX IX_RolePermissions_RoleId_PermissionId ON dbo.RolePermissions(RoleId, PermissionId) WHERE IsDeleted = 0;
CREATE NONCLUSTERED INDEX IX_Subscriptions_UserId_Status ON dbo.Subscriptions(UserId, Status);
CREATE UNIQUE NONCLUSTERED INDEX IX_Credits_UserId ON dbo.Credits(UserId) WHERE IsDeleted = 0;

CREATE NONCLUSTERED INDEX IX_CreditTransactions_UserId ON dbo.CreditTransactions(UserId);
CREATE NONCLUSTERED INDEX IX_CreditTransactions_CreditId ON dbo.CreditTransactions(CreditId);
CREATE NONCLUSTERED INDEX IX_CreditTransactions_CreatedAt ON dbo.CreditTransactions(CreatedAt);

CREATE NONCLUSTERED INDEX IX_Projects_UserId ON dbo.Projects(UserId);
CREATE NONCLUSTERED INDEX IX_ChannelAccounts_ProjectId_ChannelId ON dbo.ChannelAccounts(ProjectId, ChannelId);

CREATE NONCLUSTERED INDEX IX_Posts_ProjectId ON dbo.Posts(ProjectId);
CREATE NONCLUSTERED INDEX IX_Posts_Status ON dbo.Posts(Status);
CREATE NONCLUSTERED INDEX IX_Posts_PublishedAt ON dbo.Posts(PublishedAt);

CREATE UNIQUE NONCLUSTERED INDEX IX_PostSchedules_PostId ON dbo.PostSchedules(PostId) WHERE IsDeleted = 0;
CREATE NONCLUSTERED INDEX IX_PostSchedules_Status_ScheduledAt ON dbo.PostSchedules(Status, ScheduledAt);

CREATE NONCLUSTERED INDEX IX_AiGeneratedContents_UserId ON dbo.AiGeneratedContents(UserId);
CREATE NONCLUSTERED INDEX IX_AiGeneratedContents_CreatedAt ON dbo.AiGeneratedContents(CreatedAt);

CREATE NONCLUSTERED INDEX IX_FacebookAccounts_UserId ON dbo.FacebookAccounts(UserId);
CREATE NONCLUSTERED INDEX IX_WordPressSites_UserId ON dbo.WordPressSites(UserId);
CREATE NONCLUSTERED INDEX IX_ZaloAccounts_UserId ON dbo.ZaloAccounts(UserId);

CREATE NONCLUSTERED INDEX IX_Notifications_UserId_IsRead ON dbo.Notifications(UserId, IsRead);
CREATE UNIQUE NONCLUSTERED INDEX IX_Affiliates_UserId ON dbo.Affiliates(UserId) WHERE IsDeleted = 0;
CREATE UNIQUE NONCLUSTERED INDEX IX_Affiliates_Code ON dbo.Affiliates(Code) WHERE IsDeleted = 0;

CREATE UNIQUE NONCLUSTERED INDEX IX_Invoices_InvoiceNumber ON dbo.Invoices(InvoiceNumber) WHERE IsDeleted = 0;
CREATE NONCLUSTERED INDEX IX_Payments_TransactionId ON dbo.Payments(TransactionId) WHERE TransactionId IS NOT NULL;

CREATE UNIQUE NONCLUSTERED INDEX IX_RefreshTokens_Token ON dbo.RefreshTokens(Token) WHERE IsDeleted = 0;
CREATE UNIQUE NONCLUSTERED INDEX IX_PasswordResetTokens_Token ON dbo.PasswordResetTokens(Token) WHERE IsDeleted = 0;

CREATE NONCLUSTERED INDEX IX_AuditLogs_UserId ON dbo.AuditLogs(UserId);
CREATE NONCLUSTERED INDEX IX_AuditLogs_CreatedAt ON dbo.AuditLogs(CreatedAt);
GO

PRINT 'AutoWork database schema created successfully.';
GO
