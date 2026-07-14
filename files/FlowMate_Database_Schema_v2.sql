/* ============================================================================
   FLOWMATE DATABASE SCHEMA — v2 (merge AutoWork_Schema.sql cũ + UI/UX mới)
   ----------------------------------------------------------------------------
   Quy ước kế thừa từ bản cũ (giữ nguyên để tương thích migration/EF Core):
     - PK: UNIQUEIDENTIFIER (GUID), có DEFAULT NEWID() để tiện insert tay/test,
       EF Core khi generate migration vẫn override giá trị này bình thường.
     - Mọi bảng đều có cột audit: CreatedAt, UpdatedAt, CreatedBy, UpdatedBy,
       IsDeleted (soft-delete, không xoá cứng dữ liệu).
     - Các cột Status/Type dùng INT (ánh xạ enum bên C#) giống bản cũ, thay vì
       NVARCHAR, để giữ đúng convention EF Core cũ.
   Ghi chú: nếu team đang dùng EF Core Code-First, hãy dùng file này làm THAM
   CHIẾU để viết lại Entity Class + Migration, không nên chạy thẳng song song
   với __EFMigrationsHistory của bản cũ.
   ============================================================================ */

CREATE DATABASE FlowMateDB;
GO
USE FlowMateDB;
GO

/* ============================================================================
   PHẦN A — TÀI KHOẢN / ĐĂNG NHẬP / PHÂN QUYỀN  (giữ ~nguyên bản cũ)
   ============================================================================ */

-- Người dùng hệ thống. Hỗ trợ đăng nhập Local / Google / Facebook (đúng UI Đăng nhập-Đăng ký)
CREATE TABLE Users (
    Id                  UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    Email               NVARCHAR(256)  NOT NULL,
    PasswordHash        NVARCHAR(512)  NULL,          -- NULL nếu chỉ đăng nhập qua Google/Facebook
    FirstName           NVARCHAR(100)  NOT NULL,       -- "Họ" trong form đăng ký
    LastName            NVARCHAR(100)  NOT NULL,       -- "Tên"
    AvatarUrl           NVARCHAR(500)  NULL,
    Phone               NVARCHAR(20)   NULL,           -- xem lưu ý ở trên (NULLABLE, filtered-unique)
    IsActive            BIT            NOT NULL DEFAULT 1,
    LastLoginAt         DATETIME2      NULL,
    GoogleId            NVARCHAR(128)  NULL,
    FacebookId          NVARCHAR(128)  NULL,
    ReferralCode        NVARCHAR(32)   NULL,
    ReferredByUserId    UNIQUEIDENTIFIER NULL,
    CreatedAt           DATETIME2      NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt           DATETIME2      NULL,
    CreatedBy           UNIQUEIDENTIFIER NULL,
    UpdatedBy           UNIQUEIDENTIFIER NULL,
    IsDeleted           BIT            NOT NULL DEFAULT 0,
    CONSTRAINT PK_Users PRIMARY KEY (Id),
    CONSTRAINT FK_Users_Users_ReferredByUserId FOREIGN KEY (ReferredByUserId) REFERENCES Users(Id)
);
GO
CREATE UNIQUE INDEX IX_Users_Email ON Users(Email);
CREATE UNIQUE INDEX IX_Users_Phone_Filtered ON Users(Phone) WHERE Phone IS NOT NULL;   -- ràng buộc: nếu có SĐT thì phải duy nhất
CREATE UNIQUE INDEX IX_Users_GoogleId ON Users(GoogleId) WHERE GoogleId IS NOT NULL;
CREATE UNIQUE INDEX IX_Users_FacebookId ON Users(FacebookId) WHERE FacebookId IS NOT NULL;
CREATE UNIQUE INDEX IX_Users_ReferralCode ON Users(ReferralCode) WHERE ReferralCode IS NOT NULL;
GO

-- Vai trò hệ thống (KHÁC với vai trò trong 1 workspace — xem ProjectMembers ở Phần D)
CREATE TABLE Roles (
    Id          UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    Name        NVARCHAR(100)  NOT NULL,
    Description NVARCHAR(500)  NULL,
    IsSystem    BIT            NOT NULL DEFAULT 0,
    CreatedAt   DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt   DATETIME2 NULL, CreatedBy UNIQUEIDENTIFIER NULL, UpdatedBy UNIQUEIDENTIFIER NULL,
    IsDeleted   BIT NOT NULL DEFAULT 0,
    CONSTRAINT PK_Roles PRIMARY KEY (Id)
);
GO
CREATE UNIQUE INDEX IX_Roles_Name ON Roles(Name);
GO

CREATE TABLE Permissions (
    Id          UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    Name        NVARCHAR(200)  NOT NULL,
    Code        NVARCHAR(100)  NOT NULL,
    Description NVARCHAR(500)  NULL,
    Module      NVARCHAR(100)  NULL,
    CreatedAt   DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt   DATETIME2 NULL, CreatedBy UNIQUEIDENTIFIER NULL, UpdatedBy UNIQUEIDENTIFIER NULL,
    IsDeleted   BIT NOT NULL DEFAULT 0,
    CONSTRAINT PK_Permissions PRIMARY KEY (Id)
);
GO

CREATE TABLE RolePermissions (
    Id           UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    RoleId       UNIQUEIDENTIFIER NOT NULL,
    PermissionId UNIQUEIDENTIFIER NOT NULL,
    CreatedAt    DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt    DATETIME2 NULL, CreatedBy UNIQUEIDENTIFIER NULL, UpdatedBy UNIQUEIDENTIFIER NULL,
    IsDeleted    BIT NOT NULL DEFAULT 0,
    CONSTRAINT PK_RolePermissions PRIMARY KEY (Id),
    CONSTRAINT FK_RolePermissions_Roles FOREIGN KEY (RoleId) REFERENCES Roles(Id),
    CONSTRAINT FK_RolePermissions_Permissions FOREIGN KEY (PermissionId) REFERENCES Permissions(Id)
);
GO
CREATE UNIQUE INDEX IX_RolePermissions_RoleId_PermissionId ON RolePermissions(RoleId, PermissionId);
GO

CREATE TABLE UserRoles (
    Id        UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    UserId    UNIQUEIDENTIFIER NOT NULL,
    RoleId    UNIQUEIDENTIFIER NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL, CreatedBy UNIQUEIDENTIFIER NULL, UpdatedBy UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT PK_UserRoles PRIMARY KEY (Id),
    CONSTRAINT FK_UserRoles_Users FOREIGN KEY (UserId) REFERENCES Users(Id),
    CONSTRAINT FK_UserRoles_Roles FOREIGN KEY (RoleId) REFERENCES Roles(Id)
);
GO
CREATE UNIQUE INDEX IX_UserRoles_UserId_RoleId ON UserRoles(UserId, RoleId);
GO

-- Refresh token cho JWT
CREATE TABLE RefreshTokens (
    Id              UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    UserId          UNIQUEIDENTIFIER NOT NULL,
    Token           NVARCHAR(512) NOT NULL,
    ExpiresAt       DATETIME2 NOT NULL,
    RevokedAt       DATETIME2 NULL,
    ReplacedByToken NVARCHAR(512) NULL,
    CreatedByIp     NVARCHAR(64) NULL,
    IsRevoked       BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL, CreatedBy UNIQUEIDENTIFIER NULL, UpdatedBy UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT PK_RefreshTokens PRIMARY KEY (Id),
    CONSTRAINT FK_RefreshTokens_Users FOREIGN KEY (UserId) REFERENCES Users(Id)
);
GO

-- Quên mật khẩu (Token + OTP) — chưa thấy trong UI nhưng bắt buộc phải có cho luồng "Quên mật khẩu"
CREATE TABLE PasswordResetTokens (
    Id         UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    UserId     UNIQUEIDENTIFIER NOT NULL,
    Token      NVARCHAR(512) NOT NULL,
    OtpCode    NVARCHAR(10)  NOT NULL,
    ExpiresAt  DATETIME2 NOT NULL,
    UsedAt     DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL, CreatedBy UNIQUEIDENTIFIER NULL, UpdatedBy UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT PK_PasswordResetTokens PRIMARY KEY (Id),
    CONSTRAINT FK_PasswordResetTokens_Users FOREIGN KEY (UserId) REFERENCES Users(Id)
);
GO

CREATE TABLE Settings (
    Id          UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    [Key]       NVARCHAR(200) NOT NULL,
    Value       NVARCHAR(MAX) NOT NULL,
    Category    NVARCHAR(100) NULL,
    Description NVARCHAR(500) NULL,
    IsPublic    BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL, CreatedBy UNIQUEIDENTIFIER NULL, UpdatedBy UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT PK_Settings PRIMARY KEY (Id)
);
GO
CREATE UNIQUE INDEX IX_Settings_Key ON Settings([Key]);
GO

CREATE TABLE AuditLogs (
    Id          UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    UserId      UNIQUEIDENTIFIER NULL,
    Action      NVARCHAR(100) NOT NULL,
    EntityType  NVARCHAR(200) NOT NULL,
    EntityId    UNIQUEIDENTIFIER NOT NULL,
    OldValues   NVARCHAR(MAX) NULL,
    NewValues   NVARCHAR(MAX) NULL,
    IpAddress   NVARCHAR(45)  NULL,
    UserAgent   NVARCHAR(500) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL, CreatedBy UNIQUEIDENTIFIER NULL, UpdatedBy UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT PK_AuditLogs PRIMARY KEY (Id),
    CONSTRAINT FK_AuditLogs_Users FOREIGN KEY (UserId) REFERENCES Users(Id)
);
GO

CREATE TABLE Notifications (
    Id            UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    UserId        UNIQUEIDENTIFIER NOT NULL,
    Title         NVARCHAR(200) NOT NULL,
    Message       NVARCHAR(2000) NOT NULL,
    Type          INT NOT NULL,               -- enum: System/Billing/Post/Campaign...
    IsRead        BIT NOT NULL DEFAULT 0,
    ReadAt        DATETIME2 NULL,
    ReferenceType NVARCHAR(100) NULL,          -- VD: "Post", "Invoice"
    ReferenceId   UNIQUEIDENTIFIER NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL, CreatedBy UNIQUEIDENTIFIER NULL, UpdatedBy UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT PK_Notifications PRIMARY KEY (Id),
    CONSTRAINT FK_Notifications_Users FOREIGN KEY (UserId) REFERENCES Users(Id)
);
GO
CREATE INDEX IX_Notifications_UserId_IsRead ON Notifications(UserId, IsRead);
GO

/* ============================================================================
   PHẦN B — GÓI DỊCH VỤ & THANH TOÁN (giữ nguyên bản cũ, mở rộng nhẹ Payments)
   ============================================================================ */

-- Free / Pro / Business (khớp trang Bảng giá)
CREATE TABLE Plans (
    Id               UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    Name             NVARCHAR(100)  NOT NULL,
    Description      NVARCHAR(1000) NULL,
    Code             NVARCHAR(50)   NOT NULL,
    Price            DECIMAL(18,2)  NOT NULL,
    BillingPeriod    INT            NOT NULL,   -- enum: Monthly/Yearly
    MaxProjects      INT            NOT NULL,
    MaxChannels      INT            NOT NULL,
    MaxPostsPerMonth INT            NOT NULL,
    CreditsIncluded  INT            NOT NULL,   -- "Credit AI mở rộng" ở gói Business
    IsActive         BIT            NOT NULL DEFAULT 1,
    SortOrder        INT            NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL, CreatedBy UNIQUEIDENTIFIER NULL, UpdatedBy UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT PK_Plans PRIMARY KEY (Id)
);
GO

CREATE TABLE Subscriptions (
    Id           UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    UserId       UNIQUEIDENTIFIER NOT NULL,
    PlanId       UNIQUEIDENTIFIER NOT NULL,
    Status       INT NOT NULL,           -- enum: Trial/Active/Cancelled/Expired
    StartDate    DATETIME2 NOT NULL,
    EndDate      DATETIME2 NULL,
    CancelledAt  DATETIME2 NULL,
    AutoRenew    BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL, CreatedBy UNIQUEIDENTIFIER NULL, UpdatedBy UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT PK_Subscriptions PRIMARY KEY (Id),
    CONSTRAINT FK_Subscriptions_Plans FOREIGN KEY (PlanId) REFERENCES Plans(Id),
    CONSTRAINT FK_Subscriptions_Users FOREIGN KEY (UserId) REFERENCES Users(Id)
);
GO
CREATE INDEX IX_Subscriptions_UserId_Status ON Subscriptions(UserId, Status);
GO

CREATE TABLE Invoices (
    Id            UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    UserId        UNIQUEIDENTIFIER NOT NULL,
    SubscriptionId UNIQUEIDENTIFIER NULL,
    InvoiceNumber NVARCHAR(50)  NOT NULL,
    Amount        DECIMAL(18,2) NOT NULL,
    Tax           DECIMAL(18,2) NOT NULL DEFAULT 0,
    Total         DECIMAL(18,2) NOT NULL,
    Status        INT NOT NULL,          -- enum: Draft/Pending/Paid/Overdue/Cancelled
    DueDate       DATETIME2 NOT NULL,
    PaidAt        DATETIME2 NULL,
    Notes         NVARCHAR(1000) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL, CreatedBy UNIQUEIDENTIFIER NULL, UpdatedBy UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT PK_Invoices PRIMARY KEY (Id),
    CONSTRAINT FK_Invoices_Subscriptions FOREIGN KEY (SubscriptionId) REFERENCES Subscriptions(Id),
    CONSTRAINT FK_Invoices_Users FOREIGN KEY (UserId) REFERENCES Users(Id)
);
GO
CREATE UNIQUE INDEX IX_Invoices_InvoiceNumber ON Invoices(InvoiceNumber);
GO

-- Payments: giữ khung cũ + thêm Provider/GatewayResponse cho cổng thanh toán VN (VNPay/Momo/ZaloPay/Stripe...)
CREATE TABLE Payments (
    Id             UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    InvoiceId      UNIQUEIDENTIFIER NOT NULL,
    UserId         UNIQUEIDENTIFIER NOT NULL,
    Amount         DECIMAL(18,2) NOT NULL,
    PaymentMethod  INT NOT NULL,             -- enum: Card/BankTransfer/EWallet...
    Provider       NVARCHAR(50) NULL,        -- "VNPay","Momo","ZaloPay","Stripe"...
    TransactionId  NVARCHAR(128) NULL,       -- mã giao dịch phía cổng thanh toán
    Status         INT NOT NULL,             -- enum: Pending/Success/Failed/Refunded
    PaidAt         DATETIME2 NULL,
    FailureReason  NVARCHAR(1000) NULL,
    GatewayResponse NVARCHAR(MAX) NULL,      -- log raw response để đối soát
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL, CreatedBy UNIQUEIDENTIFIER NULL, UpdatedBy UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT PK_Payments PRIMARY KEY (Id),
    CONSTRAINT FK_Payments_Invoices FOREIGN KEY (InvoiceId) REFERENCES Invoices(Id),
    CONSTRAINT FK_Payments_Users FOREIGN KEY (UserId) REFERENCES Users(Id)
);
GO

CREATE TABLE Credits (
    Id          UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    UserId      UNIQUEIDENTIFIER NOT NULL,
    Balance     INT NOT NULL DEFAULT 0,
    TotalEarned INT NOT NULL DEFAULT 0,
    TotalUsed   INT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL, CreatedBy UNIQUEIDENTIFIER NULL, UpdatedBy UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT PK_Credits PRIMARY KEY (Id),
    CONSTRAINT FK_Credits_Users FOREIGN KEY (UserId) REFERENCES Users(Id)
);
GO
CREATE UNIQUE INDEX IX_Credits_UserId ON Credits(UserId);
GO

CREATE TABLE CreditTransactions (
    Id            UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    CreditId      UNIQUEIDENTIFIER NOT NULL,
    UserId        UNIQUEIDENTIFIER NOT NULL,
    Type          INT NOT NULL,             -- enum: Earned/Used/Refund
    Amount        INT NOT NULL,
    BalanceAfter  INT NOT NULL,
    Description   NVARCHAR(500) NULL,
    ReferenceType NVARCHAR(100) NULL,       -- VD: "AiGeneratedContent"
    ReferenceId   UNIQUEIDENTIFIER NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL, CreatedBy UNIQUEIDENTIFIER NULL, UpdatedBy UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT PK_CreditTransactions PRIMARY KEY (Id),
    CONSTRAINT FK_CreditTransactions_Credits FOREIGN KEY (CreditId) REFERENCES Credits(Id),
    CONSTRAINT FK_CreditTransactions_Users FOREIGN KEY (UserId) REFERENCES Users(Id)
);
GO

/* ============================================================================
   PHẦN C — GIỚI THIỆU / AFFILIATE (giữ nguyên bản cũ, không có trong UI hiện
   tại nhưng không xung đột nên giữ lại phòng khi cần)
   ============================================================================ */

CREATE TABLE Affiliates (
    Id             UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    UserId         UNIQUEIDENTIFIER NOT NULL,
    Code           NVARCHAR(32) NOT NULL,
    CommissionRate DECIMAL(5,4) NOT NULL,
    TotalEarnings  DECIMAL(18,2) NOT NULL DEFAULT 0,
    IsActive       BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL, CreatedBy UNIQUEIDENTIFIER NULL, UpdatedBy UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT PK_Affiliates PRIMARY KEY (Id),
    CONSTRAINT FK_Affiliates_Users FOREIGN KEY (UserId) REFERENCES Users(Id)
);
GO
CREATE UNIQUE INDEX IX_Affiliates_Code ON Affiliates(Code);
CREATE UNIQUE INDEX IX_Affiliates_UserId ON Affiliates(UserId);
GO

CREATE TABLE AffiliateLinks (
    Id              UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    AffiliateId     UNIQUEIDENTIFIER NOT NULL,
    Url             NVARCHAR(1000) NOT NULL,
    Name            NVARCHAR(200)  NOT NULL,
    Campaign        NVARCHAR(100)  NULL,
    ClickCount      INT NOT NULL DEFAULT 0,
    ConversionCount INT NOT NULL DEFAULT 0,
    IsActive        BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL, CreatedBy UNIQUEIDENTIFIER NULL, UpdatedBy UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT PK_AffiliateLinks PRIMARY KEY (Id),
    CONSTRAINT FK_AffiliateLinks_Affiliates FOREIGN KEY (AffiliateId) REFERENCES Affiliates(Id)
);
GO

CREATE TABLE AffiliateCommissions (
    Id             UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    AffiliateId    UNIQUEIDENTIFIER NOT NULL,
    ReferredUserId UNIQUEIDENTIFIER NOT NULL,
    SubscriptionId UNIQUEIDENTIFIER NULL,
    Amount         DECIMAL(18,2) NOT NULL,
    Status         INT NOT NULL,          -- enum: Pending/Approved/Paid
    PaidAt         DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL, CreatedBy UNIQUEIDENTIFIER NULL, UpdatedBy UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT PK_AffiliateCommissions PRIMARY KEY (Id),
    CONSTRAINT FK_AffiliateCommissions_Affiliates FOREIGN KEY (AffiliateId) REFERENCES Affiliates(Id),
    CONSTRAINT FK_AffiliateCommissions_Subscriptions FOREIGN KEY (SubscriptionId) REFERENCES Subscriptions(Id),
    CONSTRAINT FK_AffiliateCommissions_Users FOREIGN KEY (ReferredUserId) REFERENCES Users(Id)
);
GO

/* ============================================================================
   PHẦN D — WORKSPACE ("Việc của tôi"/Nhóm) — bảng Projects cũ + Member MỚI
   ============================================================================ */

-- = "Workspace"/"Shop" trong UI mới. Giữ tên Projects để không phá vỡ code cũ.
CREATE TABLE Projects (
    Id          UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    UserId      UNIQUEIDENTIFIER NOT NULL,     -- chủ sở hữu (Owner)
    Name        NVARCHAR(200)  NOT NULL,       -- "Việc Của Tôi"
    Description NVARCHAR(2000) NULL,
    LogoUrl     NVARCHAR(500)  NULL,
    IsActive    BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL, CreatedBy UNIQUEIDENTIFIER NULL, UpdatedBy UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT PK_Projects PRIMARY KEY (Id),
    CONSTRAINT FK_Projects_Users FOREIGN KEY (UserId) REFERENCES Users(Id)
);
GO

-- MỚI: thành viên nhóm — đáp ứng tab "Thành viên"/"Nhóm của tôi"/"Thêm thành viên"
CREATE TABLE ProjectMembers (
    Id         UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    ProjectId  UNIQUEIDENTIFIER NOT NULL,
    UserId     UNIQUEIDENTIFIER NOT NULL,
    Role       NVARCHAR(20) NOT NULL DEFAULT 'Member',  -- Owner/Admin/Member (vai trò TRONG workspace, khác Roles hệ thống)
    InvitedAt  DATETIME2 NULL,
    JoinedAt   DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL, CreatedBy UNIQUEIDENTIFIER NULL, UpdatedBy UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT PK_ProjectMembers PRIMARY KEY (Id),
    CONSTRAINT FK_ProjectMembers_Projects FOREIGN KEY (ProjectId) REFERENCES Projects(Id),
    CONSTRAINT FK_ProjectMembers_Users FOREIGN KEY (UserId) REFERENCES Users(Id),
    CONSTRAINT CK_ProjectMembers_Role CHECK (Role IN ('Owner','Admin','Member'))
);
GO
CREATE UNIQUE INDEX IX_ProjectMembers_ProjectId_UserId ON ProjectMembers(ProjectId, UserId);
GO

/* ============================================================================
   PHẦN E — KÊNH ĐĂNG BÀI / LIÊN KẾT NỀN TẢNG (giữ Channels, mở rộng ChannelAccounts)
   ============================================================================ */

-- Danh sách nền tảng hỗ trợ: Facebook, Instagram, TikTok, Zalo, WordPress...
CREATE TABLE Channels (
    Id          UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    Name        NVARCHAR(100) NOT NULL,
    Code        NVARCHAR(50)  NOT NULL,
    Description NVARCHAR(500) NULL,
    IconUrl     NVARCHAR(500) NULL,
    IsActive    BIT NOT NULL DEFAULT 1,
    SortOrder   INT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL, CreatedBy UNIQUEIDENTIFIER NULL, UpdatedBy UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT PK_Channels PRIMARY KEY (Id)
);
GO
CREATE UNIQUE INDEX IX_Channels_Code ON Channels(Code);
GO

/* ChannelAccounts: THAY THẾ cho FacebookAccounts/FacebookPages/ZaloAccounts/WordPressSites
   của bản cũ (đang bị trùng lặp thiết kế). 1 bảng generic dùng chung cho MỌI nền tảng.
   - ParentChannelAccountId: hỗ trợ phân cấp (VD: 1 tài khoản Facebook cá nhân quản lý
     nhiều Fanpage -> mỗi Fanpage là 1 dòng con trỏ về tài khoản cha).
   - AccessToken/RefreshToken/TokenExpiresAt/Scope: lưu OAuth token để gọi API đăng bài
     tự động sau này (đúng yêu cầu "liên kết nền tảng để đăng bài qua API"). */
CREATE TABLE ChannelAccounts (
    Id                     UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    ProjectId              UNIQUEIDENTIFIER NOT NULL,
    ChannelId              UNIQUEIDENTIFIER NOT NULL,
    UserId                 UNIQUEIDENTIFIER NOT NULL,     -- người thực hiện kết nối
    ParentChannelAccountId UNIQUEIDENTIFIER NULL,
    Name                   NVARCHAR(200) NOT NULL,        -- tên hiển thị (VD: "FlowMate Store")
    ExternalId             NVARCHAR(128) NULL,            -- id/handle từ nền tảng (Page Id, Zalo OA Id...)
    ProfileUrl             NVARCHAR(500) NULL,
    AvatarUrl              NVARCHAR(500) NULL,
    AccessToken            NVARCHAR(MAX) NULL,            -- mã hoá ở tầng ứng dụng trước khi lưu
    RefreshToken           NVARCHAR(MAX) NULL,
    TokenExpiresAt         DATETIME2 NULL,
    Scope                  NVARCHAR(500) NULL,
    IsActive               BIT NOT NULL DEFAULT 1,
    LastSyncedAt           DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL, CreatedBy UNIQUEIDENTIFIER NULL, UpdatedBy UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT PK_ChannelAccounts PRIMARY KEY (Id),
    CONSTRAINT FK_ChannelAccounts_Channels FOREIGN KEY (ChannelId) REFERENCES Channels(Id),
    CONSTRAINT FK_ChannelAccounts_Projects FOREIGN KEY (ProjectId) REFERENCES Projects(Id),
    CONSTRAINT FK_ChannelAccounts_Users FOREIGN KEY (UserId) REFERENCES Users(Id),
    CONSTRAINT FK_ChannelAccounts_Parent FOREIGN KEY (ParentChannelAccountId) REFERENCES ChannelAccounts(Id)
);
GO
CREATE INDEX IX_ChannelAccounts_ProjectId ON ChannelAccounts(ProjectId);
CREATE INDEX IX_ChannelAccounts_ChannelId ON ChannelAccounts(ChannelId);
GO

/* ============================================================================
   PHẦN F — BRAND MEMORY (hoàn toàn mới so với bản cũ)
   ============================================================================ */

CREATE TABLE VoiceSampleTemplates (
    Id         UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    SampleText NVARCHAR(500) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL, CreatedBy UNIQUEIDENTIFIER NULL, UpdatedBy UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT PK_VoiceSampleTemplates PRIMARY KEY (Id)
);
GO

CREATE TABLE BrandStyles (
    Id        UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    StyleName NVARCHAR(50) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL, CreatedBy UNIQUEIDENTIFIER NULL, UpdatedBy UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT PK_BrandStyles PRIMARY KEY (Id)
);
GO
CREATE UNIQUE INDEX IX_BrandStyles_StyleName ON BrandStyles(StyleName);
GO

CREATE TABLE ToneKeywords (
    Id        UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    Keyword   NVARCHAR(50) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL, CreatedBy UNIQUEIDENTIFIER NULL, UpdatedBy UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT PK_ToneKeywords PRIMARY KEY (Id)
);
GO
CREATE UNIQUE INDEX IX_ToneKeywords_Keyword ON ToneKeywords(Keyword);
GO

CREATE TABLE CTATemplates (
    Id      UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    CTAText NVARCHAR(100) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL, CreatedBy UNIQUEIDENTIFIER NULL, UpdatedBy UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT PK_CTATemplates PRIMARY KEY (Id)
);
GO
CREATE UNIQUE INDEX IX_CTATemplates_CTAText ON CTATemplates(CTAText);
GO

-- Dùng chung cho cả Brand Memory lẫn từng Post
CREATE TABLE Hashtags (
    Id  UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    Tag NVARCHAR(50) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL, CreatedBy UNIQUEIDENTIFIER NULL, UpdatedBy UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT PK_Hashtags PRIMARY KEY (Id)
);
GO
CREATE UNIQUE INDEX IX_Hashtags_Tag ON Hashtags(Tag);
GO

-- Hồ sơ thương hiệu — 1-1 với Projects (nằm trong "Hồ sơ tài khoản")
CREATE TABLE BrandProfiles (
    Id               UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    ProjectId        UNIQUEIDENTIFIER NOT NULL,
    LogoUrl          NVARCHAR(500)  NULL,
    BusinessName     NVARCHAR(150)  NOT NULL,
    BrandName        NVARCHAR(150)  NOT NULL,
    Industry         NVARCHAR(100)  NULL,
    FoundingYear     SMALLINT       NULL,
    ShortDescription NVARCHAR(1000) NULL,
    Address          NVARCHAR(300)  NULL,
    ContactPhone     NVARCHAR(20)   NULL,
    ContactEmail     NVARCHAR(255)  NULL,
    Website          NVARCHAR(255)  NULL,
    VoiceSampleId    UNIQUEIDENTIFIER NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL, CreatedBy UNIQUEIDENTIFIER NULL, UpdatedBy UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT PK_BrandProfiles PRIMARY KEY (Id),
    CONSTRAINT FK_BrandProfiles_Projects FOREIGN KEY (ProjectId) REFERENCES Projects(Id),
    CONSTRAINT FK_BrandProfiles_VoiceSample FOREIGN KEY (VoiceSampleId) REFERENCES VoiceSampleTemplates(Id),
    CONSTRAINT UQ_BrandProfiles_ProjectId UNIQUE (ProjectId)
);
GO

CREATE TABLE BrandProfileStyles (          -- N-N, business rule: tối đa 2 (trigger bên dưới)
    BrandProfileId UNIQUEIDENTIFIER NOT NULL,
    StyleId        UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT PK_BrandProfileStyles PRIMARY KEY (BrandProfileId, StyleId),
    CONSTRAINT FK_BPStyles_Brand FOREIGN KEY (BrandProfileId) REFERENCES BrandProfiles(Id) ON DELETE CASCADE,
    CONSTRAINT FK_BPStyles_Style FOREIGN KEY (StyleId) REFERENCES BrandStyles(Id)
);
GO

CREATE TABLE BrandProfileKeywords (         -- N-N, khuyến nghị 3-5 từ (trigger bên dưới)
    BrandProfileId UNIQUEIDENTIFIER NOT NULL,
    ToneKeywordId  UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT PK_BrandProfileKeywords PRIMARY KEY (BrandProfileId, ToneKeywordId),
    CONSTRAINT FK_BPKeywords_Brand FOREIGN KEY (BrandProfileId) REFERENCES BrandProfiles(Id) ON DELETE CASCADE,
    CONSTRAINT FK_BPKeywords_Keyword FOREIGN KEY (ToneKeywordId) REFERENCES ToneKeywords(Id)
);
GO

CREATE TABLE BrandColors (
    Id             UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    BrandProfileId UNIQUEIDENTIFIER NOT NULL,
    ColorHex       NVARCHAR(9)  NOT NULL,
    ColorType      NVARCHAR(20) NOT NULL DEFAULT 'Secondary',
    SortOrder      INT NOT NULL DEFAULT 0,
    CONSTRAINT PK_BrandColors PRIMARY KEY (Id),
    CONSTRAINT FK_BrandColors_Brand FOREIGN KEY (BrandProfileId) REFERENCES BrandProfiles(Id) ON DELETE CASCADE,
    CONSTRAINT CK_BrandColors_Type CHECK (ColorType IN ('Primary','Secondary'))
);
GO

CREATE TABLE BrandFonts (
    Id             UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    BrandProfileId UNIQUEIDENTIFIER NOT NULL,
    FontName       NVARCHAR(100) NOT NULL,
    UsageType      NVARCHAR(20)  NOT NULL,       -- Title/Body
    CONSTRAINT PK_BrandFonts PRIMARY KEY (Id),
    CONSTRAINT FK_BrandFonts_Brand FOREIGN KEY (BrandProfileId) REFERENCES BrandProfiles(Id) ON DELETE CASCADE,
    CONSTRAINT CK_BrandFonts_Usage CHECK (UsageType IN ('Title','Body'))
);
GO

CREATE TABLE BrandCTAs (
    BrandProfileId UNIQUEIDENTIFIER NOT NULL,
    CTAId          UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT PK_BrandCTAs PRIMARY KEY (BrandProfileId, CTAId),
    CONSTRAINT FK_BrandCTAs_Brand FOREIGN KEY (BrandProfileId) REFERENCES BrandProfiles(Id) ON DELETE CASCADE,
    CONSTRAINT FK_BrandCTAs_CTA FOREIGN KEY (CTAId) REFERENCES CTATemplates(Id)
);
GO

CREATE TABLE BrandHashtags (
    BrandProfileId UNIQUEIDENTIFIER NOT NULL,
    HashtagId      UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT PK_BrandHashtags PRIMARY KEY (BrandProfileId, HashtagId),
    CONSTRAINT FK_BrandHashtags_Brand FOREIGN KEY (BrandProfileId) REFERENCES BrandProfiles(Id) ON DELETE CASCADE,
    CONSTRAINT FK_BrandHashtags_Tag FOREIGN KEY (HashtagId) REFERENCES Hashtags(Id)
);
GO

CREATE TABLE ProductCategories (
    Id             UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    BrandProfileId UNIQUEIDENTIFIER NOT NULL,
    CategoryName   NVARCHAR(100) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL, CreatedBy UNIQUEIDENTIFIER NULL, UpdatedBy UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT PK_ProductCategories PRIMARY KEY (Id),
    CONSTRAINT FK_ProdCat_Brand FOREIGN KEY (BrandProfileId) REFERENCES BrandProfiles(Id) ON DELETE CASCADE,
    CONSTRAINT UQ_ProdCat_Name UNIQUE (BrandProfileId, CategoryName)
);
GO

CREATE TABLE Products (
    Id                     UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    BrandProfileId         UNIQUEIDENTIFIER NOT NULL,
    ProductCategoryId      UNIQUEIDENTIFIER NULL,
    ProductName            NVARCHAR(200) NOT NULL,
    Price                  DECIMAL(18,2) NULL,
    AIGeneratedDescription NVARCHAR(MAX) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL, CreatedBy UNIQUEIDENTIFIER NULL, UpdatedBy UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT PK_Products PRIMARY KEY (Id),
    CONSTRAINT FK_Products_Brand FOREIGN KEY (BrandProfileId) REFERENCES BrandProfiles(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Products_Category FOREIGN KEY (ProductCategoryId) REFERENCES ProductCategories(Id)
);
GO

/* ============================================================================
   PHẦN G — NỘI DUNG AI (giữ nguyên bản cũ) & THƯ VIỆN MEDIA (mở rộng)
   ============================================================================ */

CREATE TABLE AiPrompts (
    Id        UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    UserId    UNIQUEIDENTIFIER NULL,
    ProjectId UNIQUEIDENTIFIER NULL,
    Name      NVARCHAR(200) NOT NULL,
    Template  NVARCHAR(MAX) NOT NULL,
    Category  NVARCHAR(100) NULL,
    IsSystem  BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL, CreatedBy UNIQUEIDENTIFIER NULL, UpdatedBy UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT PK_AiPrompts PRIMARY KEY (Id),
    CONSTRAINT FK_AiPrompts_Projects FOREIGN KEY (ProjectId) REFERENCES Projects(Id),
    CONSTRAINT FK_AiPrompts_Users FOREIGN KEY (UserId) REFERENCES Users(Id)
);
GO

CREATE TABLE AiGeneratedContents (
    Id           UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    AiPromptId   UNIQUEIDENTIFIER NOT NULL,
    UserId       UNIQUEIDENTIFIER NOT NULL,
    ProjectId    UNIQUEIDENTIFIER NULL,
    Input        NVARCHAR(MAX) NOT NULL,
    Output       NVARCHAR(MAX) NULL,
    TokensUsed   INT NOT NULL DEFAULT 0,
    Status       INT NOT NULL,           -- enum: Pending/Success/Failed
    ErrorMessage NVARCHAR(2000) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL, CreatedBy UNIQUEIDENTIFIER NULL, UpdatedBy UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT PK_AiGeneratedContents PRIMARY KEY (Id),
    CONSTRAINT FK_AiGeneratedContents_AiPrompts FOREIGN KEY (AiPromptId) REFERENCES AiPrompts(Id),
    CONSTRAINT FK_AiGeneratedContents_Projects FOREIGN KEY (ProjectId) REFERENCES Projects(Id),
    CONSTRAINT FK_AiGeneratedContents_Users FOREIGN KEY (UserId) REFERENCES Users(Id)
);
GO

-- MediaFiles: thêm IsAIGenerated + AIPrompt để phục vụ tính năng "Tạo ảnh AI"
-- (thay vì tạo bảng AIGeneratedImages riêng, tái dùng luôn bảng thư viện media chung)
CREATE TABLE MediaFiles (
    Id            UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    UserId        UNIQUEIDENTIFIER NOT NULL,
    ProjectId     UNIQUEIDENTIFIER NULL,
    FileName      NVARCHAR(255)  NOT NULL,
    FileUrl       NVARCHAR(1000) NOT NULL,
    MimeType      NVARCHAR(100)  NOT NULL,
    FileSize      BIGINT NOT NULL,
    ThumbnailUrl  NVARCHAR(1000) NULL,
    Width         INT NULL,
    Height        INT NULL,
    IsAIGenerated BIT NOT NULL DEFAULT 0,
    AIPrompt      NVARCHAR(MAX) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL, CreatedBy UNIQUEIDENTIFIER NULL, UpdatedBy UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT PK_MediaFiles PRIMARY KEY (Id),
    CONSTRAINT FK_MediaFiles_Projects FOREIGN KEY (ProjectId) REFERENCES Projects(Id),
    CONSTRAINT FK_MediaFiles_Users FOREIGN KEY (UserId) REFERENCES Users(Id)
);
GO

/* ============================================================================
   PHẦN H — CHIẾN DỊCH & TIMELINE (hoàn toàn mới)
   ============================================================================ */

CREATE TABLE CampaignGoals (
    Id       UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    GoalName NVARCHAR(100) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL, CreatedBy UNIQUEIDENTIFIER NULL, UpdatedBy UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT PK_CampaignGoals PRIMARY KEY (Id)
);
GO
CREATE UNIQUE INDEX IX_CampaignGoals_GoalName ON CampaignGoals(GoalName);
GO

CREATE TABLE PromotionTypes (
    Id       UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    TypeName NVARCHAR(100) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL, CreatedBy UNIQUEIDENTIFIER NULL, UpdatedBy UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT PK_PromotionTypes PRIMARY KEY (Id)
);
GO
CREATE UNIQUE INDEX IX_PromotionTypes_TypeName ON PromotionTypes(TypeName);
GO

CREATE TABLE TimelineTemplateTypes (
    Id          UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    TypeName    NVARCHAR(50) NOT NULL,      -- Classic Campaign / Roadmap / Calendar
    Description NVARCHAR(300) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL, CreatedBy UNIQUEIDENTIFIER NULL, UpdatedBy UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT PK_TimelineTemplateTypes PRIMARY KEY (Id)
);
GO
CREATE UNIQUE INDEX IX_TimelineTemplateTypes_TypeName ON TimelineTemplateTypes(TypeName);
GO

CREATE TABLE Campaigns (
    Id              UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    ProjectId       UNIQUEIDENTIFIER NOT NULL,
    Name            NVARCHAR(200) NOT NULL,
    GoalId          UNIQUEIDENTIFIER NOT NULL,
    PromotionTypeId UNIQUEIDENTIFIER NOT NULL,
    DiscountPercent DECIMAL(5,2)  NULL,
    DiscountAmount  DECIMAL(18,2) NULL,
    MinOrderAmount  DECIMAL(18,2) NULL,
    StartDate       DATE NOT NULL,
    EndDate         DATE NOT NULL,
    NumberOfPosts   INT NOT NULL DEFAULT 1,
    Status          INT NOT NULL,          -- enum: Draft/Active/Ended
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL, CreatedBy UNIQUEIDENTIFIER NULL, UpdatedBy UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT PK_Campaigns PRIMARY KEY (Id),
    CONSTRAINT FK_Campaigns_Projects FOREIGN KEY (ProjectId) REFERENCES Projects(Id),
    CONSTRAINT FK_Campaigns_Goals FOREIGN KEY (GoalId) REFERENCES CampaignGoals(Id),
    CONSTRAINT FK_Campaigns_PromoType FOREIGN KEY (PromotionTypeId) REFERENCES PromotionTypes(Id),
    CONSTRAINT CK_Campaigns_Dates CHECK (EndDate >= StartDate),
    CONSTRAINT CK_Campaigns_NumberOfPosts CHECK (NumberOfPosts > 0)
);
GO

-- N-N: 1 chiến dịch có thể chạy trên nhiều tài khoản kênh cụ thể (không chỉ 1 nền tảng chung chung)
CREATE TABLE CampaignChannelAccounts (
    CampaignId       UNIQUEIDENTIFIER NOT NULL,
    ChannelAccountId UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT PK_CampaignChannelAccounts PRIMARY KEY (CampaignId, ChannelAccountId),
    CONSTRAINT FK_CampChAcc_Campaign FOREIGN KEY (CampaignId) REFERENCES Campaigns(Id) ON DELETE CASCADE,
    CONSTRAINT FK_CampChAcc_ChannelAccount FOREIGN KEY (ChannelAccountId) REFERENCES ChannelAccounts(Id)
);
GO

CREATE TABLE Timelines (
    Id             UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    ProjectId      UNIQUEIDENTIFIER NOT NULL,
    CampaignId     UNIQUEIDENTIFIER NULL,       -- NULL nếu timeline độc lập, không gắn chiến dịch
    Name           NVARCHAR(200) NOT NULL,
    TemplateTypeId UNIQUEIDENTIFIER NOT NULL,
    StartDate      DATE NOT NULL,
    EndDate        DATE NOT NULL,
    Status         INT NOT NULL,               -- enum: Draft/Active/Completed
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL, CreatedBy UNIQUEIDENTIFIER NULL, UpdatedBy UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT PK_Timelines PRIMARY KEY (Id),
    CONSTRAINT FK_Timelines_Projects FOREIGN KEY (ProjectId) REFERENCES Projects(Id),
    CONSTRAINT FK_Timelines_Campaigns FOREIGN KEY (CampaignId) REFERENCES Campaigns(Id),
    CONSTRAINT FK_Timelines_TemplateType FOREIGN KEY (TemplateTypeId) REFERENCES TimelineTemplateTypes(Id),
    CONSTRAINT CK_Timelines_Dates CHECK (EndDate >= StartDate)
);
GO

/* ============================================================================
   PHẦN I — BÀI VIẾT & ĐĂNG BÀI (giữ khung PostContents/PostSchedules/PostLogs
   của bản cũ, thêm PostChannelAccounts để hỗ trợ đăng 1 bài lên NHIỀU nền tảng)
   ============================================================================ */

CREATE TABLE Posts (
    Id              UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    ProjectId       UNIQUEIDENTIFIER NOT NULL,
    TimelineId      UNIQUEIDENTIFIER NULL,      -- NULL nếu là "bài viết đơn" độc lập
    VoiceSampleId   UNIQUEIDENTIFIER NULL,
    Title           NVARCHAR(500) NOT NULL,
    DayNumber       INT NULL,                   -- vị trí Day 1/2/3 trong timeline dạng Classic
    ScheduledAt     DATETIME2 NULL,
    Status          INT NOT NULL,               -- enum: Draft/Scheduled/Published/Failed
    TrackingEnabled BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL, CreatedBy UNIQUEIDENTIFIER NULL, UpdatedBy UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT PK_Posts PRIMARY KEY (Id),
    CONSTRAINT FK_Posts_Projects FOREIGN KEY (ProjectId) REFERENCES Projects(Id),
    CONSTRAINT FK_Posts_Timelines FOREIGN KEY (TimelineId) REFERENCES Timelines(Id),
    CONSTRAINT FK_Posts_VoiceSample FOREIGN KEY (VoiceSampleId) REFERENCES VoiceSampleTemplates(Id)
);
GO

-- Nội dung bài viết dạng khối (Text/Image/Video), 1 Post có nhiều PostContents (giữ nguyên bản cũ)
CREATE TABLE PostContents (
    Id          UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    PostId      UNIQUEIDENTIFIER NOT NULL,
    ContentType INT NOT NULL,                 -- enum: Text/Image/Video
    Content     NVARCHAR(MAX) NOT NULL,
    SortOrder   INT NOT NULL DEFAULT 0,
    MediaFileId UNIQUEIDENTIFIER NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL, CreatedBy UNIQUEIDENTIFIER NULL, UpdatedBy UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT PK_PostContents PRIMARY KEY (Id),
    CONSTRAINT FK_PostContents_MediaFiles FOREIGN KEY (MediaFileId) REFERENCES MediaFiles(Id),
    CONSTRAINT FK_PostContents_Posts FOREIGN KEY (PostId) REFERENCES Posts(Id)
);
GO

-- N-N: Post <-> Hashtags
CREATE TABLE PostHashtags (
    PostId    UNIQUEIDENTIFIER NOT NULL,
    HashtagId UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT PK_PostHashtags PRIMARY KEY (PostId, HashtagId),
    CONSTRAINT FK_PostHashtags_Posts FOREIGN KEY (PostId) REFERENCES Posts(Id) ON DELETE CASCADE,
    CONSTRAINT FK_PostHashtags_Tags FOREIGN KEY (HashtagId) REFERENCES Hashtags(Id)
);
GO

/* ----------------------------------------------------------------------------
   *** BẢNG TRUNG GIAN N-N CHÍNH (đúng yêu cầu "mỗi sản phẩm/nền tảng có thông
   tin riêng" của bạn trong nhóm) ***
   1 Post => chọn NHIỀU ChannelAccount (Facebook Page A, Instagram B...)
          => MỖI ChannelAccount có Status/ExternalPostId/PublishedUrl RIÊNG
   ---------------------------------------------------------------------------- */
CREATE TABLE PostChannelAccounts (
    Id               UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    PostId           UNIQUEIDENTIFIER NOT NULL,
    ChannelAccountId UNIQUEIDENTIFIER NOT NULL,
    Status           INT NOT NULL,               -- enum: Scheduled/Published/Failed (riêng theo từng kênh)
    ExternalPostId   NVARCHAR(128)  NULL,        -- id bài viết trả về từ API nền tảng
    PublishedUrl     NVARCHAR(1000) NULL,
    PublishedAt      DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL, CreatedBy UNIQUEIDENTIFIER NULL, UpdatedBy UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT PK_PostChannelAccounts PRIMARY KEY (Id),
    CONSTRAINT FK_PostChAcc_Posts FOREIGN KEY (PostId) REFERENCES Posts(Id) ON DELETE CASCADE,
    CONSTRAINT FK_PostChAcc_ChannelAccounts FOREIGN KEY (ChannelAccountId) REFERENCES ChannelAccounts(Id),
    CONSTRAINT UQ_PostChannelAccounts UNIQUE (PostId, ChannelAccountId)   -- không đăng trùng 1 kênh 2 lần cho cùng 1 bài
);
GO

-- Lịch sử đăng bài / retry (giữ nguyên bản cũ, gắn theo từng PostChannelAccount thay vì Post)
CREATE TABLE PostSchedules (
    Id                  UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    PostChannelAccountId UNIQUEIDENTIFIER NOT NULL,
    ScheduledAt         DATETIME2 NOT NULL,
    Status              INT NOT NULL,           -- enum: Pending/Executed/Failed
    ExecutedAt          DATETIME2 NULL,
    FailureReason       NVARCHAR(1000) NULL,
    RetryCount          INT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL, CreatedBy UNIQUEIDENTIFIER NULL, UpdatedBy UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT PK_PostSchedules PRIMARY KEY (Id),
    CONSTRAINT FK_PostSchedules_PostChannelAccounts FOREIGN KEY (PostChannelAccountId) REFERENCES PostChannelAccounts(Id)
);
GO

-- Log chi tiết quá trình xử lý bài viết (AI sinh nội dung, đăng bài, lỗi API...)
CREATE TABLE PostLogs (
    Id        UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    PostId    UNIQUEIDENTIFIER NOT NULL,
    Action    NVARCHAR(100) NOT NULL,
    Message   NVARCHAR(2000) NOT NULL,
    Level     INT NOT NULL,                -- enum: Info/Warning/Error
    Details   NVARCHAR(MAX) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL, CreatedBy UNIQUEIDENTIFIER NULL, UpdatedBy UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT PK_PostLogs PRIMARY KEY (Id),
    CONSTRAINT FK_PostLogs_Posts FOREIGN KEY (PostId) REFERENCES Posts(Id)
);
GO

-- Số liệu hiệu suất theo từng cặp Post-ChannelAccount (1-1 với PostChannelAccounts)
CREATE TABLE PostAnalytics (
    Id                   UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    PostChannelAccountId UNIQUEIDENTIFIER NOT NULL,
    Reach     INT NOT NULL DEFAULT 0,
    Likes     INT NOT NULL DEFAULT 0,
    Comments  INT NOT NULL DEFAULT 0,
    Shares    INT NOT NULL DEFAULT 0,
    RecordedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_PostAnalytics PRIMARY KEY (Id),
    CONSTRAINT FK_PostAnalytics_PostChannelAccounts FOREIGN KEY (PostChannelAccountId) REFERENCES PostChannelAccounts(Id) ON DELETE CASCADE,
    CONSTRAINT UQ_PostAnalytics_PostChannelAccountId UNIQUE (PostChannelAccountId)
);
GO

/* ============================================================================
   PHẦN J — TRIGGER RÀNG BUỘC NGHIỆP VỤ (không diễn đạt được bằng CHECK thuần)
   ============================================================================ */

-- Tối đa 2 phong cách thương hiệu (Brand Memory > Phong cách)
CREATE TRIGGER TR_BrandProfileStyles_MaxTwo
ON BrandProfileStyles
AFTER INSERT
AS
BEGIN
    IF EXISTS (
        SELECT BrandProfileId FROM BrandProfileStyles
        WHERE BrandProfileId IN (SELECT BrandProfileId FROM inserted)
        GROUP BY BrandProfileId HAVING COUNT(*) > 2
    )
    BEGIN
        RAISERROR('Mỗi thương hiệu chỉ được chọn tối đa 2 phong cách.', 16, 1);
        ROLLBACK TRANSACTION;
    END
END;
GO

-- Tối đa 5 từ khóa giọng văn (khuyến nghị 3-5)
CREATE TRIGGER TR_BrandProfileKeywords_MaxFive
ON BrandProfileKeywords
AFTER INSERT
AS
BEGIN
    IF EXISTS (
        SELECT BrandProfileId FROM BrandProfileKeywords
        WHERE BrandProfileId IN (SELECT BrandProfileId FROM inserted)
        GROUP BY BrandProfileId HAVING COUNT(*) > 5
    )
    BEGIN
        RAISERROR('Mỗi thương hiệu chỉ nên chọn tối đa 5 từ khóa mô tả giọng văn.', 16, 1);
        ROLLBACK TRANSACTION;
    END
END;
GO

/* ============================================================================
   PHẦN K — INDEX GỢI Ý CHO TRUY VẤN THƯỜNG DÙNG
   ============================================================================ */
CREATE INDEX IX_ProjectMembers_UserId       ON ProjectMembers(UserId);
CREATE INDEX IX_BrandProfiles_ProjectId     ON BrandProfiles(ProjectId);
CREATE INDEX IX_Products_BrandProfileId     ON Products(BrandProfileId);
CREATE INDEX IX_Campaigns_ProjectId         ON Campaigns(ProjectId);
CREATE INDEX IX_Timelines_CampaignId        ON Timelines(CampaignId);
CREATE INDEX IX_Posts_TimelineId            ON Posts(TimelineId);
CREATE INDEX IX_Posts_ScheduledAt           ON Posts(ScheduledAt);
CREATE INDEX IX_PostChannelAccounts_PostId  ON PostChannelAccounts(PostId);
CREATE INDEX IX_ChannelAccounts_ParentId    ON ChannelAccounts(ParentChannelAccountId);
GO
