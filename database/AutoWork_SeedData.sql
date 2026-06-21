-- =============================================
-- AutoWork — Seed Data (autowork.com.vn)
-- IDEMPOTENT: chạy nhiều lần an toàn (SSMS / Azure Data Studio)
-- Chạy SAU AutoWork_Schema.sql hoặc dotnet ef database update
-- Users/permissions/demo: chạy AutoWork.API (DataSeeder)
-- =============================================

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

DECLARE @Now DATETIME2 = SYSUTCDATETIME();

-- ========== ROLES ==========
IF NOT EXISTS (SELECT 1 FROM [Roles] WHERE [Id] = '22222222-2222-2222-2222-222222222222')
    INSERT INTO [Roles] ([Id],[Name],[Description],[IsSystem],[CreatedAt],[IsDeleted])
    VALUES ('22222222-2222-2222-2222-222222222222', N'SuperAdmin', N'Full system access', 1, @Now, 0);

IF NOT EXISTS (SELECT 1 FROM [Roles] WHERE [Id] = '33333333-3333-3333-3333-333333333333')
    INSERT INTO [Roles] ([Id],[Name],[Description],[IsSystem],[CreatedAt],[IsDeleted])
    VALUES ('33333333-3333-3333-3333-333333333333', N'Admin', N'Administrative access', 1, @Now, 0);

IF NOT EXISTS (SELECT 1 FROM [Roles] WHERE [Id] = '44444444-4444-4444-4444-444444444444')
    INSERT INTO [Roles] ([Id],[Name],[Description],[IsSystem],[CreatedAt],[IsDeleted])
    VALUES ('44444444-4444-4444-4444-444444444444', N'User', N'Standard user access', 1, @Now, 0);

-- ========== PLANS ==========
IF NOT EXISTS (SELECT 1 FROM [Plans] WHERE [Id] = '55555555-5555-5555-5555-555555555555')
    INSERT INTO [Plans] ([Id],[Name],[Code],[Description],[Price],[BillingPeriod],[MaxProjects],[MaxChannels],[MaxPostsPerMonth],[CreditsIncluded],[IsActive],[SortOrder],[CreatedAt],[IsDeleted])
    VALUES ('55555555-5555-5555-5555-555555555555', N'Free', N'Free', N'Miễn phí trải nghiệm — 100 credits/tháng', 0, 1, 1, 2, 50, 100, 1, 1, @Now, 0);

IF NOT EXISTS (SELECT 1 FROM [Plans] WHERE [Id] = '66666666-6666-6666-6666-666666666666')
    INSERT INTO [Plans] ([Id],[Name],[Code],[Description],[Price],[BillingPeriod],[MaxProjects],[MaxChannels],[MaxPostsPerMonth],[CreditsIncluded],[IsActive],[SortOrder],[CreatedAt],[IsDeleted])
    VALUES ('66666666-6666-6666-6666-666666666666', N'Premium', N'Starter', N'AI viết & đăng bài đa kênh Facebook, WordPress, Zalo OA', 299000, 1, 3, 5, 200, 1000, 1, 2, @Now, 0);

IF NOT EXISTS (SELECT 1 FROM [Plans] WHERE [Id] = '77777777-7777-7777-7777-777777777777')
    INSERT INTO [Plans] ([Id],[Name],[Code],[Description],[Price],[BillingPeriod],[MaxProjects],[MaxChannels],[MaxPostsPerMonth],[CreditsIncluded],[IsActive],[SortOrder],[CreatedAt],[IsDeleted])
    VALUES ('77777777-7777-7777-7777-777777777777', N'Pro', N'Pro', N'Cho doanh nghiệp đang tăng trưởng', 499000, 1, 10, 20, 1000, 10000, 1, 3, @Now, 0);

IF NOT EXISTS (SELECT 1 FROM [Plans] WHERE [Id] = '88888888-8888-8888-8888-888888888888')
    INSERT INTO [Plans] ([Id],[Name],[Code],[Description],[Price],[BillingPeriod],[MaxProjects],[MaxChannels],[MaxPostsPerMonth],[CreditsIncluded],[IsActive],[SortOrder],[CreatedAt],[IsDeleted])
    VALUES ('88888888-8888-8888-8888-888888888888', N'Business', N'Business', N'Không giới hạn credits + hỗ trợ ưu tiên', 1999000, 1, 100, 100, 10000, 2147483647, 1, 4, @Now, 0);

-- ========== CHANNELS ==========
IF NOT EXISTS (SELECT 1 FROM [Channels] WHERE [Id] = 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa')
    INSERT INTO [Channels] ([Id],[Name],[Code],[Description],[IsActive],[SortOrder],[CreatedAt],[IsDeleted])
    VALUES ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', N'Facebook', N'facebook', N'Đăng bài Facebook Fanpage + Auto Comment', 1, 1, @Now, 0);

IF NOT EXISTS (SELECT 1 FROM [Channels] WHERE [Id] = 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb')
    INSERT INTO [Channels] ([Id],[Name],[Code],[Description],[IsActive],[SortOrder],[CreatedAt],[IsDeleted])
    VALUES ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', N'WordPress', N'wordpress', N'Đăng bài SEO WordPress / WooCommerce', 1, 2, @Now, 0);

IF NOT EXISTS (SELECT 1 FROM [Channels] WHERE [Id] = 'cccccccc-cccc-cccc-cccc-cccccccccccc')
    INSERT INTO [Channels] ([Id],[Name],[Code],[Description],[IsActive],[SortOrder],[CreatedAt],[IsDeleted])
    VALUES ('cccccccc-cccc-cccc-cccc-cccccccccccc', N'Zalo', N'zalo', N'Đăng bài Zalo Official Account', 1, 3, @Now, 0);

-- ========== SETTINGS ==========
MERGE [Settings] AS t
USING (VALUES
    (N'App.Name',                      N'AutoWork',              N'General',   N'Tên ứng dụng',              1),
    (N'App.SupportEmail',              N'support@autowork.com.vn', N'General', N'Email hỗ trợ',              1),
    (N'App.SupportPhone',              N'0394932696',            N'General',   N'Hotline',                   1),
    (N'App.DefaultLanguage',           N'vi-VN',                 N'General',   N'Ngôn ngữ mặc định',         1),
    (N'App.PremiumTrialDays',          N'1',                     N'General',   N'Dùng thử Premium (ngày)',   1),
    (N'Auth.JwtExpiryMinutes',         N'60',                    N'Auth',      N'JWT expiry',                0),
    (N'Auth.RefreshTokenExpiryDays',   N'7',                     N'Auth',      N'Refresh token expiry',      0),
    (N'Credits.DefaultFreeCredits',    N'100',                   N'Credits',   N'Credits gói Free',          0),
    (N'Credits.AiGenerateCost',        N'5',                     N'Credits',   N'Chi phí tạo AI',            0),
    (N'Credits.PostPublishCost',       N'2',                     N'Credits',   N'Chi phí đăng bài',          0),
    (N'Payment.Currency',              N'VND',                   N'Payment',   N'Tiền tệ',                   1),
    (N'Affiliate.DefaultCommissionRate', N'0.30',               N'Affiliate', N'Hoa hồng 30%',              0),
    (N'Affiliate.CookieDays',          N'30',                    N'Affiliate', N'Cookie giới thiệu',         0),
    (N'Affiliate.CommissionMonths',    N'12',                    N'Affiliate', N'Thời hạn hoa hồng',         0)
) AS s ([Key], [Value], [Category], [Description], [IsPublic])
ON t.[Key] = s.[Key]
WHEN NOT MATCHED BY TARGET THEN
    INSERT ([Id],[Key],[Value],[Category],[Description],[IsPublic],[CreatedAt],[IsDeleted])
    VALUES (NEWID(), s.[Key], s.[Value], s.[Category], s.[Description], s.[IsPublic], @Now, 0);

-- ========== AI PROMPTS ==========
IF NOT EXISTS (SELECT 1 FROM [AiPrompts] WHERE [Id] = 'dddddddd-dddd-dddd-dddd-dddddddddddd')
    INSERT INTO [AiPrompts] ([Id],[UserId],[ProjectId],[Name],[Template],[Category],[IsSystem],[CreatedAt],[IsDeleted])
    VALUES ('dddddddd-dddd-dddd-dddd-dddddddddddd', NULL, NULL, N'Blog Post Generator', N'Write a blog post about {{topic}}. Keywords: {{keywords}} Tone: {{tone}}', N'Content', 1, @Now, 0);

IF NOT EXISTS (SELECT 1 FROM [AiPrompts] WHERE [Id] = 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee')
    INSERT INTO [AiPrompts] ([Id],[UserId],[ProjectId],[Name],[Template],[Category],[IsSystem],[CreatedAt],[IsDeleted])
    VALUES ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee', NULL, NULL, N'Social Media Post', N'Create social post for {{topic}}. Platform: {{platform}} CTA: {{cta}}', N'Social', 1, @Now, 0);

IF NOT EXISTS (SELECT 1 FROM [AiPrompts] WHERE [Id] = 'ffffffff-ffff-ffff-ffff-ffffffffffff')
    INSERT INTO [AiPrompts] ([Id],[UserId],[ProjectId],[Name],[Template],[Category],[IsSystem],[CreatedAt],[IsDeleted])
    VALUES ('ffffffff-ffff-ffff-ffff-ffffffffffff', NULL, NULL, N'SEO Article', N'Write SEO article about {{topic}}. Focus: {{keywords}}', N'SEO', 1, @Now, 0);

IF NOT EXISTS (SELECT 1 FROM [AiPrompts] WHERE [Id] = '10101010-1010-1010-1010-101010101010')
    INSERT INTO [AiPrompts] ([Id],[UserId],[ProjectId],[Name],[Template],[Category],[IsSystem],[CreatedAt],[IsDeleted])
    VALUES ('10101010-1010-1010-1010-101010101010', NULL, NULL, N'Product Description', N'Product description for {{topic}}. Tone: {{tone}}', N'E-commerce', 1, @Now, 0);

IF NOT EXISTS (SELECT 1 FROM [AiPrompts] WHERE [Id] = '20202020-2020-2020-2020-202020202020')
    INSERT INTO [AiPrompts] ([Id],[UserId],[ProjectId],[Name],[Template],[Category],[IsSystem],[CreatedAt],[IsDeleted])
    VALUES ('20202020-2020-2020-2020-202020202020', NULL, NULL, N'Facebook Fanpage Bán Hàng', N'Viết bài Fanpage bán {{topic}}. CTA: {{cta}}', N'Facebook', 1, @Now, 0);

IF NOT EXISTS (SELECT 1 FROM [AiPrompts] WHERE [Id] = '21212121-2121-2121-2121-212121212121')
    INSERT INTO [AiPrompts] ([Id],[UserId],[ProjectId],[Name],[Template],[Category],[IsSystem],[CreatedAt],[IsDeleted])
    VALUES ('21212121-2121-2121-2121-212121212121', NULL, NULL, N'WordPress SEO + WooCommerce', N'Bài SEO WordPress về {{topic}}. Keyword: {{keywords}}', N'WordPress', 1, @Now, 0);

IF NOT EXISTS (SELECT 1 FROM [AiPrompts] WHERE [Id] = '24242424-2424-2424-2424-242424242424')
    INSERT INTO [AiPrompts] ([Id],[UserId],[ProjectId],[Name],[Template],[Category],[IsSystem],[CreatedAt],[IsDeleted])
    VALUES ('24242424-2424-2424-2424-242424242424', NULL, NULL, N'Zalo OA Chăm Sóc Khách', N'Tin Zalo OA về {{topic}}. CTA: {{cta}}', N'Zalo', 1, @Now, 0);

IF NOT EXISTS (SELECT 1 FROM [AiPrompts] WHERE [Id] = '23232323-2323-2323-2323-232323232323')
    INSERT INTO [AiPrompts] ([Id],[UserId],[ProjectId],[Name],[Template],[Category],[IsSystem],[CreatedAt],[IsDeleted])
    VALUES ('23232323-2323-2323-2323-232323232323', NULL, NULL, N'Auto Comment Fanpage', N'Trả lời comment: {{input}}. Tone: {{tone}}', N'Facebook', 1, @Now, 0);

COMMIT TRANSACTION;

PRINT N'✓ Seed hoàn tất (idempotent — bỏ qua bản ghi đã tồn tại).';
PRINT N'  Chạy AutoWork.API để tạo users, permissions và demo data nếu chưa có.';
GO
