/* ============================================================================
   FlowMateDB — EF Core compatibility layer (idempotent)
   ============================================================================ */
USE [FlowMateDB];
GO
SET NOCOUNT ON;

IF COL_LENGTH('dbo.Posts', 'ChannelAccountId') IS NULL
    ALTER TABLE dbo.Posts ADD ChannelAccountId UNIQUEIDENTIFIER NULL;
IF COL_LENGTH('dbo.Posts', 'ExternalPostId') IS NULL
    ALTER TABLE dbo.Posts ADD ExternalPostId NVARCHAR(128) NULL;
IF COL_LENGTH('dbo.Posts', 'PublishedUrl') IS NULL
    ALTER TABLE dbo.Posts ADD PublishedUrl NVARCHAR(1000) NULL;
IF COL_LENGTH('dbo.Posts', 'PublishedAt') IS NULL
    ALTER TABLE dbo.Posts ADD PublishedAt DATETIME2 NULL;
GO

IF OBJECT_ID(N'dbo.FacebookAccounts', N'U') IS NULL
CREATE TABLE dbo.FacebookAccounts (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    UserId UNIQUEIDENTIFIER NOT NULL,
    FacebookUserId NVARCHAR(128) NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    Email NVARCHAR(256) NULL,
    ProfilePictureUrl NVARCHAR(500) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    TokenExpiresAt DATETIME2 NULL,
    LastSyncedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2 NULL,
    CreatedBy UNIQUEIDENTIFIER NULL,
    UpdatedBy UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0
);
GO

IF OBJECT_ID(N'dbo.FacebookPages', N'U') IS NULL
CREATE TABLE dbo.FacebookPages (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    FacebookAccountId UNIQUEIDENTIFIER NOT NULL,
    PageId NVARCHAR(128) NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    Category NVARCHAR(200) NULL,
    ProfilePictureUrl NVARCHAR(500) NULL,
    IsConnected BIT NOT NULL DEFAULT 1,
    LastSyncedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2 NULL,
    CreatedBy UNIQUEIDENTIFIER NULL,
    UpdatedBy UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0
);
GO

IF OBJECT_ID(N'dbo.WordPressSites', N'U') IS NULL
CREATE TABLE dbo.WordPressSites (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    UserId UNIQUEIDENTIFIER NOT NULL,
    SiteUrl NVARCHAR(500) NOT NULL,
    SiteName NVARCHAR(200) NOT NULL,
    Username NVARCHAR(100) NOT NULL,
    ApplicationPassword NVARCHAR(MAX) NOT NULL DEFAULT '',
    IsWooCommerce BIT NOT NULL DEFAULT 0,
    IsConnected BIT NOT NULL DEFAULT 1,
    LastSyncedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2 NULL,
    CreatedBy UNIQUEIDENTIFIER NULL,
    UpdatedBy UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0
);
GO

IF OBJECT_ID(N'dbo.WordPressPosts', N'U') IS NULL
CREATE TABLE dbo.WordPressPosts (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    WordPressSiteId UNIQUEIDENTIFIER NOT NULL,
    ExternalPostId NVARCHAR(128) NOT NULL DEFAULT '',
    Title NVARCHAR(500) NOT NULL,
    Excerpt NVARCHAR(MAX) NULL,
    Status INT NOT NULL,
    Permalink NVARCHAR(1000) NULL,
    PublishedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2 NULL,
    CreatedBy UNIQUEIDENTIFIER NULL,
    UpdatedBy UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0
);
GO

IF OBJECT_ID(N'dbo.ZaloAccounts', N'U') IS NULL
CREATE TABLE dbo.ZaloAccounts (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    UserId UNIQUEIDENTIFIER NOT NULL,
    ZaloUserId NVARCHAR(128) NOT NULL,
    DisplayName NVARCHAR(200) NOT NULL,
    AvatarUrl NVARCHAR(500) NULL,
    IsConnected BIT NOT NULL DEFAULT 1,
    TokenExpiresAt DATETIME2 NULL,
    LastSyncedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2 NULL,
    CreatedBy UNIQUEIDENTIFIER NULL,
    UpdatedBy UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0
);
GO

IF OBJECT_ID(N'dbo.ZaloPosts', N'U') IS NULL
CREATE TABLE dbo.ZaloPosts (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    ZaloAccountId UNIQUEIDENTIFIER NOT NULL,
    ExternalPostId NVARCHAR(128) NULL,
    Content NVARCHAR(MAX) NOT NULL,
    Status INT NOT NULL,
    PublishedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2 NULL,
    CreatedBy UNIQUEIDENTIFIER NULL,
    UpdatedBy UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0
);
GO

PRINT N'FlowMate EF compatibility applied.';
GO
