# Kế hoạch refactor frontend theo mẫu FlowMate (Bước 3 - CHƯA CODE)

## 1) Mục tiêu và phạm vi

- Refactor giao diện frontend trong `EXE/frontend/AutoWork.Web/wwwroot` để bám sát phong cách mẫu FlowMate.
- Chỉ thay đổi frontend: HTML/CSS/JS phía client.
- Không thay đổi backend layer: `Controllers/`, `Services/`, `Repositories/`, `Data/` (DbContext, Migrations), DTO/API contract.

## 2) Nguyên tắc an toàn bắt buộc khi sửa

- Giữ nguyên route/frontend URL:
  - `/index.html`, `/login.html`, `/register.html`, `/app/*.html`.
- Giữ nguyên endpoint API đang gọi trong JS:
  - `/api/*` và cơ chế proxy `api.js`.
- Giữ nguyên khóa session trong `localStorage`:
  - `accessToken`, `refreshToken`, `expiresAt`, `user`, `autowork_demo_state`.
- Giữ nguyên tên function JS đang dùng:
  - `Auth.*`, `apiGet/apiPost/apiPut/apiDelete`, `renderAppLayout`, `setPageTitle`, `setPageContent`.
- Không đổi tên các `id` đang bind sự kiện trực tiếp trong script nếu không update đồng bộ tại chính file đó.
- Không sửa bất kỳ file backend nào ngoài thư mục frontend.

## 3) Thứ tự thực thi dự kiến (sửa từng trang một)

1. Nền tảng giao diện chung (design tokens + layout shell).
2. Public pages: login, register, landing.
3. Dashboard.
4. AI Content (flow step 3 theo mẫu).
5. Posts (flow step 4 theo mẫu).
6. Schedules (flow step 2/5 theo mẫu timeline).
7. Các module còn lại theo style chung: channels, media, notifications, settings, plans, credits, affiliate, admin.
8. Rà soát nhất quán và responsive.

## 4) Danh sách file sẽ sửa và nội dung sửa

### 4.1 Nền tảng giao diện chung

- `frontend/AutoWork.Web/wwwroot/css/app.css`
  - Chuyển sang design token và component style theo mẫu FlowMate:
    - màu tím chủ đạo, neutral sáng, card bo góc lớn, chip/pill, form control, step panel.
  - Chuẩn hóa sidebar/topbar/banner/tab/card/button theo ảnh mẫu.
  - Bổ sung responsive cho layout 3 cột (timeline/editor/preview) và màn nhỏ.
- `frontend/AutoWork.Web/wwwroot/js/layout.js`
  - Điều chỉnh shell layout để giống mẫu:
    - sidebar trái + topbar tìm kiếm + banner gói + khu vực action.
  - Giữ nguyên:
    - điều hướng menu hiện tại,
    - id container `pageTitle`, `headerActions`, `pageContent`,
    - nút logout `btnLogout`.

### 4.2 Public pages

- `frontend/AutoWork.Web/wwwroot/login.html`
  - Chuyển layout sang 2 cột như mẫu login.
  - Giữ nguyên behavior login:
    - `loginForm`, `email`, `password`, `btnSubmit`, `error`, redirect logic.
- `frontend/AutoWork.Web/wwwroot/register.html`
  - Chuyển layout sang style register theo mẫu.
  - Giữ nguyên submit payload register hiện tại và id input đang dùng.
- `frontend/AutoWork.Web/wwwroot/index.html`
  - Đổi landing theo visual mẫu (hero/feature/benefit/pricing style FlowMate).
  - Giữ nguyên luồng gọi `GET /api/plans` và render pricing.

### 4.3 App pages khớp trực tiếp mẫu

- `frontend/AutoWork.Web/wwwroot/app/dashboard.html`
  - Re-layout dashboard theo bố cục card KPI + panel chính kiểu mẫu.
  - Giữ nguyên gọi `GET /api/dashboard/stats`.
- `frontend/AutoWork.Web/wwwroot/app/ai.html`
  - Áp giao diện step campaign với khối upload ảnh + mô tả AI.
  - Giữ nguyên:
    - API `/api/Ai/generate`, `/api/Ai/prompts`, `/api/Ai/history`,
    - action tạo post từ AI qua `/api/Posts`.
- `frontend/AutoWork.Web/wwwroot/app/posts.html`
  - Đổi sang bố cục editor + preview social gần mẫu.
  - Giữ nguyên:
    - CRUD post qua `/api/Posts*`,
    - modal tạo/sửa/xóa/lên lịch.
- `frontend/AutoWork.Web/wwwroot/app/schedules.html`
  - Đổi sang giao diện chọn mẫu timeline + timeline list.
  - Giữ nguyên:
    - `/api/Schedules*`, `/api/Posts*`,
    - các action edit/delete/create schedule.

### 4.4 App pages chưa có mẫu trực tiếp (sẽ đồng bộ style chung FlowMate)

- `frontend/AutoWork.Web/wwwroot/app/channels.html`
  - Đồng bộ style (card/chip/button/sidebar/topbar), không đổi nghiệp vụ connect demo/API.
- `frontend/AutoWork.Web/wwwroot/app/media.html`
  - Đồng bộ style grid upload/gallery, không đổi flow upload hiện tại.
- `frontend/AutoWork.Web/wwwroot/app/notifications.html`
  - Đồng bộ style list thông báo, không đổi API mark read.
- `frontend/AutoWork.Web/wwwroot/app/settings.html`
  - Đồng bộ style tab form, không đổi logic forgot-password/reset demo.
- `frontend/AutoWork.Web/wwwroot/app/plans.html`
  - Đồng bộ style pricing/subscription theo visual FlowMate.
- `frontend/AutoWork.Web/wwwroot/app/credits.html`
  - Đồng bộ style wallet/transactions, giữ nguyên flow hiện tại.
- `frontend/AutoWork.Web/wwwroot/app/affiliate.html`
  - Đồng bộ style card/list, giữ nguyên API affiliate.
- `frontend/AutoWork.Web/wwwroot/app/admin.html`
  - Đồng bộ style bảng quản trị, giữ nguyên API admin/users.

## 5) Các mục UI mẫu chưa có backend tương ứng (không tự thêm backend)

- Brand Memory module đầy đủ (hồ sơ doanh nghiệp, kênh bán, nhận diện thương hiệu, tiến độ).
- Workspace cộng tác sâu (`Thành viên`, `Nhóm của tôi`, `Lĩnh vực/dự án`) theo nghĩa domain mới.
- Dashboard quản lý công việc cá nhân (task/worktime/calendar domain riêng).
- Social auth Google/Facebook.

Các mục này chỉ được:
- ghi chú UI placeholder trong phạm vi frontend nếu cần, hoặc
- chờ bạn xác nhận mở rộng backend ở pha sau.

## 6) Rủi ro tiềm ẩn

- Đổi cấu trúc HTML có thể làm gãy event listeners đang bind theo `id`.
- Đổi class/DOM tree có thể làm hỏng thao tác modal/toast/tab đang dùng trong `demo.js`.
- Refactor `layout.js` có thể gây lỗi render trang nếu thiếu container `pageTitle/headerActions/pageContent`.
- CSS mới có thể đè style cũ ngoài ý muốn (regression responsive hoặc trạng thái hover/focus).
- Trang có fallback demo + API thật có thể lệch hiển thị nếu markup không bao phủ cả 2 mode.

## 7) Cách giảm rủi ro trong lúc thực thi

- Sửa từng trang độc lập, không refactor đồng loạt.
- Sau mỗi trang:
  - build project frontend host (`AutoWork.Web`) để kiểm tra compile.
  - rà nhanh thao tác chính của trang đó (load dữ liệu, submit, modal, điều hướng).
- Khi phải đổi `id/class`, cập nhật đồng thời selector trong cùng file script; không đổi contract API.
- Không đụng backend trừ khi có yêu cầu rõ ràng từ bạn.

## 8) Kết quả đầu ra dự kiến của Bước 4

- Frontend `EXE` đồng bộ style FlowMate ở toàn bộ màn hình hiện có.
- Nghiệp vụ backend giữ nguyên, endpoint và dữ liệu request/response giữ nguyên.
- Có log rõ trang nào đã sửa và selector nào buộc phải chỉnh để không vỡ hành vi.
