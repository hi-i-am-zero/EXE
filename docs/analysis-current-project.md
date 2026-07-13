# Phân tích hiện trạng dự án `EXE` (Bước 1)

## 1) Tổng quan kiến trúc thư mục

`EXE` đang theo mô hình nhiều project (Clean Architecture tương đối rõ), tách frontend và backend:

- `frontend/AutoWork.Web`
  - **Vai trò:** frontend hiện tại.
  - **Kiểu frontend:** static HTML/CSS/JS (không dùng Razor Pages/MVC Views/React).
  - **Điểm vào:** `Program.cs` (serve static files + endpoint `/api/config` + proxy `/backend-api/{**path}`).
  - **UI chính:** `wwwroot/index.html`, `wwwroot/login.html`, `wwwroot/register.html`, `wwwroot/app/*.html`.
- `src/AutoWork.API`
  - **Vai trò:** backend Web API.
  - **Chứa:** `Controllers/`, middleware, authorization, `Program.cs`.
  - **Đặc điểm:** route API chính `/api/*`, Swagger, JWT auth, CORS, rate limit, Hangfire dashboard.
- `src/AutoWork.Application`
  - **Vai trò:** application layer (DTOs, Commands/Queries, handlers, validators, interfaces).
  - **Mẫu:** MediatR + FluentValidation + AutoMapper.
- `src/AutoWork.Domain`
  - **Vai trò:** domain layer (Entities, Enums, base entity).
- `src/AutoWork.Persistence`
  - **Vai trò:** persistence layer.
  - **Chứa:** `Context/ApplicationDbContext.cs`, `Repositories/`, `Configurations/`, `Migrations/`, `Seed/`.
- `src/AutoWork.Infrastructure`
  - **Vai trò:** hạ tầng tích hợp ngoài (AI providers, payment providers, email, storage, SignalR hub, Hangfire jobs, JWT).
- `src/AutoWork.Shared`
  - **Vai trò:** hằng số dùng chung (`AppPermissions`, `AppRoles`), model response (`ApiResponse`, `PaginatedResult`), enum dùng chung.
- `tests/*`
  - Unit test và integration test.

## 2) Xác định rõ frontend vs backend layer

### Frontend layer

- Vị trí chính: `frontend/AutoWork.Web/wwwroot`.
- Cấu trúc:
  - `wwwroot/*.html`: landing/login/register.
  - `wwwroot/app/*.html`: các màn hình sau đăng nhập.
  - `wwwroot/js/*.js`: `api.js`, `auth.js`, `layout.js`, `demo.js`.
  - `wwwroot/css/app.css`: design system hiện tại.
- Cách hoạt động:
  - Gọi API qua `fetch` + helper `apiGet/apiPost/apiPut/apiDelete`.
  - Token lưu `localStorage`.
  - Có cơ chế demo fallback (`DemoStore`) ở nhiều màn hình.

### Backend layer

- Entry API: `src/AutoWork.API/Program.cs`.
- API Controllers: `src/AutoWork.API/Controllers/*.cs`.
- Business orchestration: `src/AutoWork.Application` (MediatR commands/queries, DTOs, validators).
- Data access:
  - `src/AutoWork.Persistence/Context/ApplicationDbContext.cs`.
  - `src/AutoWork.Persistence/Repositories/*.cs`.
  - `src/AutoWork.Persistence/Migrations/*.cs`.
  - `src/AutoWork.Persistence/Configurations/*.cs`.
- Integration/infra:
  - `src/AutoWork.Infrastructure/Services/*`.
  - `src/AutoWork.Infrastructure/SignalR/NotificationHub.cs`.
  - `src/AutoWork.Infrastructure/Jobs/*` (Hangfire jobs).

## 3) Danh sách chức năng nghiệp vụ hiện có (module/API/UI)

Ghi chú:
- API route base mặc định từ `ApiControllerBase` là `api/[controller]` nếu controller không override `[Route]`.
- Cột “UI hiện có” là màn hình frontend trong `frontend/AutoWork.Web/wwwroot`.

### 3.1 Auth & tài khoản

- **Đăng ký**
  - API: `POST /api/auth/register`
  - UI: `/register.html`
- **Đăng nhập**
  - API: `POST /api/auth/login`
  - UI: `/login.html`
- **Refresh token**
  - API: `POST /api/auth/refresh`
  - UI: gọi ngầm trong `js/api.js`
- **Quên mật khẩu**
  - API: `POST /api/auth/forgot-password`
  - UI: `/app/settings.html` (tab bảo mật)
- **Đặt lại mật khẩu**
  - API: `POST /api/auth/reset-password`
  - UI: **chưa có màn hình nhập OTP/reset riêng** trong frontend hiện tại

### 3.2 Dashboard

- **Thống kê tổng quan**
  - API: `GET /api/dashboard/stats`
  - UI: `/app/dashboard.html`

### 3.3 Posts (bài viết)

- **Danh sách bài viết**
  - API: `GET /api/posts?page=&pageSize=&status=`
  - UI: `/app/posts.html`
- **Chi tiết bài viết**
  - API: `GET /api/posts/{id}`
  - UI: modal trong `/app/posts.html`
- **Tạo bài viết**
  - API: `POST /api/posts`
  - UI: `/app/posts.html` và tạo từ `/app/ai.html`
- **Cập nhật bài viết**
  - API: `PUT /api/posts/{id}`
  - UI: `/app/posts.html`
- **Xóa bài viết**
  - API: `DELETE /api/posts/{id}`
  - UI: `/app/posts.html`

### 3.4 Schedules (lên lịch)

- **Danh sách lịch đăng**
  - API: `GET /api/schedules?count=`
  - UI: `/app/schedules.html`
- **Chi tiết lịch**
  - API: `GET /api/schedules/{id}`
  - UI: gián tiếp trong `/app/schedules.html`
- **Tạo lịch**
  - API: `POST /api/schedules`
  - UI: `/app/schedules.html`, `/app/posts.html` (lên lịch nhanh)
- **Cập nhật lịch**
  - API: `PUT /api/schedules/{id}`
  - UI: `/app/schedules.html`
- **Hủy/xóa lịch**
  - API: `DELETE /api/schedules/{id}`
  - UI: `/app/schedules.html`

### 3.5 Plans & subscriptions

- **Lấy danh sách gói**
  - API: `GET /api/plans` (anonymous)
  - UI: `/index.html`, `/app/plans.html`
- **Lấy gói theo id**
  - API: `GET /api/plans/{id}` (anonymous)
  - UI: chưa có màn chi tiết riêng
- **Subscription hiện tại**
  - API: `GET /api/plans/subscriptions`
  - UI: `/app/plans.html`
- **Đăng ký subscription**
  - API: `POST /api/plans/subscriptions`
  - UI: `/app/plans.html`
- **Hủy subscription**
  - API: `DELETE /api/plans/subscriptions`
  - UI: `/app/plans.html`

### 3.6 Credits

- **Số dư credit**
  - API: `GET /api/credits`
  - UI: `/app/credits.html`
- **Lịch sử giao dịch credit**
  - API: `GET /api/credits/transactions?page=&pageSize=`
  - UI: `/app/credits.html`

### 3.7 Payments

- **Danh sách thanh toán**
  - API: `GET /api/payments`
  - UI: **chưa có màn hình thanh toán thật riêng**
- **Tạo payment**
  - API: `POST /api/payments`
  - UI: **chưa có flow gọi API này ở frontend hiện tại** (credits/plans hiện đang demo payment UI)
- **Lấy invoice**
  - API: `GET /api/payments/invoices/{invoiceId}`
  - UI: chưa có
- **Webhook cổng thanh toán**
  - API:
    - `POST /api/payments/webhooks/vnpay`
    - `POST /api/payments/webhooks/momo`
    - `POST /api/payments/webhooks/zalopay`
  - UI: không áp dụng (server callback)

### 3.8 AI Content

- **Sinh nội dung AI**
  - API: `POST /api/ai/generate`
  - UI: `/app/ai.html`
- **Lịch sử AI**
  - API: `GET /api/ai/history?page=&pageSize=`
  - UI: `/app/ai.html` (tab history)
- **Danh sách prompts**
  - API: `GET /api/ai/prompts`
  - UI: `/app/ai.html`
- **Tạo prompt**
  - API: `POST /api/ai/prompts`
  - UI: `/app/ai.html`
- **Xóa prompt**
  - API: `DELETE /api/ai/prompts/{id}`
  - UI: `/app/ai.html`

### 3.9 Media

- **Danh sách media**
  - API: `GET /api/media?folder=`
  - UI: `/app/media.html` (hiện chủ yếu dùng `DemoStore`, chưa load danh sách media thật từ API)
- **Upload media**
  - API: `POST /api/media/upload`
  - UI: `/app/media.html` (đã có gọi upload thật qua `fetch`)
- **Xóa media**
  - API: `DELETE /api/media/{id}`
  - UI: **chưa gọi API delete thật**, hiện xóa cục bộ demo

### 3.10 Notifications

- **Danh sách thông báo**
  - API: `GET /api/notifications?page=&pageSize=&isRead=`
  - UI: `/app/notifications.html`
- **Đếm unread**
  - API: `GET /api/notifications/unread-count`
  - UI: **chưa gọi endpoint này**, badge sidebar lấy từ `DemoStore`
- **Đánh dấu đã đọc**
  - API: `PUT /api/notifications/{id}/read`
  - UI: `/app/notifications.html`
- **Đánh dấu đọc tất cả**
  - API: `PUT /api/notifications/read-all`
  - UI: `/app/notifications.html`

### 3.11 Affiliate

- **Thông tin affiliate profile**
  - API: `GET /api/affiliates`
  - UI: `/app/affiliate.html`
- **Danh sách affiliate links**
  - API: `GET /api/affiliates/links`
  - UI: `/app/affiliate.html`
- **Tạo affiliate link**
  - API: `POST /api/affiliates/links`
  - UI: `/app/affiliate.html`
- **Danh sách hoa hồng**
  - API: `GET /api/affiliates/commissions`
  - UI: `/app/affiliate.html`

### 3.12 Channels tích hợp ngoài

- **Facebook**
  - API:
    - `GET /api/facebook/connect`
    - `POST /api/facebook/connect`
    - `GET /api/facebook/pages`
    - `POST /api/facebook/pages/{accountId}/sync`
    - `POST /api/facebook/publish`
  - UI: `/app/channels.html` (hiện chủ yếu demo, chưa đi flow OAuth/API thật)
- **WordPress**
  - API:
    - `GET /api/wordpress/sites`
    - `POST /api/wordpress/sites`
    - `PUT /api/wordpress/sites/{id}`
    - `DELETE /api/wordpress/sites/{id}`
    - `GET /api/wordpress/sites/{siteId}/posts`
    - `POST /api/wordpress/posts`
  - UI: `/app/channels.html` có khối WordPress demo, **chưa có CRUD site thật**
- **Zalo**
  - API:
    - `GET /api/zalo/connect`
    - `POST /api/zalo/accounts`
    - `GET /api/zalo/accounts`
    - `GET /api/zalo/accounts/{accountId}/posts`
    - `POST /api/zalo/posts`
  - UI: `/app/channels.html` (demo connect/publish)

### 3.13 Admin & Users

- **Admin dashboard**
  - API: `GET /api/admin/dashboard`
  - UI: `/app/admin.html`
- **Khóa/mở user**
  - API: `PUT /api/admin/users/{id}/status`
  - UI: `/app/admin.html`
- **Users list**
  - API: `GET /api/users?page=&pageSize=&search=`
  - UI: `/app/admin.html`
- **User detail**
  - API: `GET /api/users/{id}`
  - UI: chưa có màn detail riêng
- **Create user**
  - API: `POST /api/users`
  - UI: modal trong `/app/admin.html`
- **Update user**
  - API: `PUT /api/users/{id}`
  - UI: chưa có UI edit profile user trong admin
- **Delete user**
  - API: `DELETE /api/users/{id}` (SuperAdmin)
  - UI: chưa có nút delete trên admin page hiện tại

## 4) Công nghệ frontend đang dùng (kèm version)

### 4.1 Runtime/frontend host

- ASP.NET Core Web (`net8.0`) project `AutoWork.Web` để serve static frontend.
- Minimal API endpoint:
  - `GET /api/config`
  - `GET /app` redirect dashboard
  - Proxy dev: `/backend-api/{**path}`.

### 4.2 UI stack phía client

- **Vanilla HTML/CSS/JavaScript** (không React/Vue/Angular/jQuery).
- **Fetch API** cho HTTP request.
- **LocalStorage** cho session + demo state.
- **Chart.js `4.4.1`** (dashboard charts).
- **Bootstrap Icons `1.11.3`** (icon font qua CDN).
- **Google Font: Inter**.
- **CSS custom design system** trong `wwwroot/css/app.css` (biến màu, spacing, component classes).

### 4.3 Realtime và nền backend liên quan frontend

- Backend có **SignalR NotificationHub** tại `/hubs/notifications`.
- Frontend hiện tại **chưa có code client SignalR** (không thấy khởi tạo `HubConnection`).

## 5) Các điểm quan trọng cho bước refactor UI sắp tới

- Frontend hiện tại có nhiều phần đang ở chế độ demo (`DemoStore`) và chưa nối đủ API thật.
- Có một số endpoint backend đầy đủ nhưng UI mới chỉ cover một phần.
- Khi refactor UI cần giữ nguyên:
  - route trang (`/app/*.html`, `/login.html`, `/register.html`),
  - contract API (`/api/*` payload),
  - auth token flow (`accessToken`, `refreshToken`, `expiresAt` trong localStorage),
  - các selector/id đang được JS hiện tại tham chiếu nếu chưa đổi đồng bộ script.

