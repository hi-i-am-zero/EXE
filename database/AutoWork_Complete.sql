-- =============================================
-- AutoWork — Database Setup Hoàn Chỉnh
-- Mô phỏng nền tảng autowork.com.vn
-- SQL Server 2019+
--
-- CÁCH 1 (khuyến nghị): dotnet ef database update
-- CÁCH 2: Chạy file này trong SSMS / Azure Data Studio
-- =============================================

USE [master];
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'AutoWorkDb')
BEGIN
    CREATE DATABASE [AutoWorkDb];
    PRINT N'Created database AutoWorkDb';
END
GO

USE [AutoWorkDb];
GO

-- Bước 1: Schema (35 bảng + indexes) — nội dung từ EF Migration
-- Chạy file: database/AutoWork_Schema.sql

-- Bước 2: Seed dữ liệu tham chiếu
-- Chạy file: database/AutoWork_SeedData.sql

-- Bước 3: Users + permissions + demo data
-- Chạy AutoWork.API một lần (InitializeDatabaseAsync + DataSeeder)

/*
========================================
DANH SÁCH 35 BẢNG
========================================
Auth & RBAC (7):
  Users, Roles, Permissions, UserRoles, RolePermissions,
  RefreshTokens, PasswordResetTokens

Billing & Credits (6):
  Plans, Subscriptions, Credits, CreditTransactions,
  Invoices, Payments

Workspace & Publishing (8):
  Projects, Channels, ChannelAccounts, Posts,
  PostContents, PostSchedules, PostLogs, MediaFiles

AI (2):
  AiPrompts, AiGeneratedContents

Facebook (2):
  FacebookAccounts, FacebookPages

WordPress (2):
  WordPressSites, WordPressPosts

Zalo (2):
  ZaloAccounts, ZaloPosts

Affiliate (3):
  Affiliates, AffiliateLinks, AffiliateCommissions

System (3):
  Notifications, Settings, AuditLogs

EF (1):
  __EFMigrationsHistory

========================================
TÀI KHOẢN SAU KHI CHẠY API (DataSeeder)
========================================
Super Admin : admin@autowork.com  / Admin@123!
Demo User   : demo@autowork.com   / Demo@123!

========================================
GÓI DỊCH VỤ (autowork.com.vn)
========================================
Free     : 0đ      — 100 credits, 1 dự án, 2 kênh
Premium  : 299.000đ — 1000 credits, 3 dự án, 5 kênh
Pro      : 499.000đ — 10000 credits, 10 dự án, 20 kênh
Business : 1.999.000đ — unlimited credits

========================================
TÍNH NĂNG DATABASE HỖ TRỢ
========================================
- AI viết content đa kênh (Facebook, WordPress, Zalo OA)
- Lên lịch đăng bài tự động (PostSchedules)
- Credits / thanh toán VNPay, MoMo, ZaloPay
- Affiliate 30% hoa hồng, cookie 30 ngày
- Auto Comment Fanpage (AI prompt)
- WordPress SEO + WooCommerce
- Audit log, notifications, media library
*/

PRINT N'Xem hướng dẫn trong comment block của file AutoWork_Complete.sql';
GO
