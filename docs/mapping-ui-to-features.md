# Mapping UX/UI -> Chức năng dự án (Bước 2)

## Trạng thái dữ liệu đầu vào

- Thư mục `C:\Users\nxtru\OneDrive\Máy tính\Project EXE\ui ux` hiện tại vẫn rỗng.
- Phân tích Bước 2 trong file này được thực hiện dựa trên bộ screenshot bạn gửi trực tiếp trong chat (11 ảnh).

## 1) Danh sách màn hình trong bộ UX/UI đã nhận

- Màn hình đăng nhập (layout 2 cột, ảnh minh họa lớn bên phải).
- Màn hình đăng ký (form chi tiết + social signup).
- Landing page marketing (phiên bản mobile dài, nhiều section).
- Dashboard/Trang chủ kiểu quản lý công việc (thẻ KPI + lịch).
- Luồng chiến dịch tự động - Bước 1: Kế hoạch nội dung (mục tiêu, ưu đãi, thời gian, số bài).
- Luồng chiến dịch tự động - Bước 2: Chọn mẫu timeline (Classic/Roadmap/Calendar).
- Luồng chiến dịch tự động - Bước 3: AI tạo mô tả sản phẩm (upload ảnh, sinh mô tả).
- Luồng chiến dịch tự động - Bước 4: Tạo bài viết đơn/chỉnh nội dung (multi-platform + preview).
- Luồng chiến dịch tự động - Bước 5: Timeline editor + xuất bản.
- Brand Memory - Form nhập hồ sơ thương hiệu (phong cách, giọng văn, màu, CTA, hashtag).
- Brand Memory - Tổng quan tiến độ (doanh nghiệp, liên hệ, kênh bán, sản phẩm, nhận diện).

## 2) Component và pattern UI chính

- Sidebar trái cố định: logo, hồ sơ nhanh, menu module, cài đặt.
- Topbar ngang: ô tìm kiếm, thông báo gói FREE, CTA nâng cấp, nút thêm thành viên.
- Tab navigation ngang theo ngữ cảnh workspace (`Tổng quan`, `Thành viên`, `Nhóm của tôi`, `Lĩnh vực, dự án`).
- Card-based layout: card bo góc lớn, viền mỏng, khoảng trắng rộng, chia panel rõ ràng.
- Wizard theo bước với CTA lớn cuối trang (`Tiếp tục`, `Đăng bài`).
- Chip/pill selector cho mục tiêu chiến dịch, tone, hashtag, ưu đãi.
- Composer 2-3 cột: danh sách timeline bên trái, editor giữa, preview mạng xã hội bên phải.
- Upload gallery dạng thumbnail (nút thêm ảnh, xóa ảnh, giới hạn số lượng).
- Preview bài đăng social và block lịch đăng bên phải.
- Progress widget dạng vòng tròn (Brand Memory completion).

## 3) Style guide rút ra từ mẫu

- Màu chủ đạo: tím/indigo (button, active state, icon nhấn).
- Màu nền: trắng + xám rất nhạt, nhấn bằng border mảnh và shadow nhẹ.
- Màu semantic: xanh lá cho trạng thái thành công/đang hoạt động, cam cho cảnh báo nhẹ.
- Typography: sans-serif hiện đại, heading đậm, body cỡ vừa, mật độ chữ thoáng.
- Bo góc: đồng nhất, thiên hướng mềm (button/card/input đều bo tròn).
- Spacing: hệ thống theo lưới đều, ưu tiên khoảng trắng lớn để tách nhóm thông tin.
- Tương tác: hover/active tinh tế, không dùng hiệu ứng nặng.

## 4) Mapping với chức năng hiện có trong `EXE`

### (a) Chức năng có sẵn + có mẫu UI tương ứng

- `Login UI mẫu` <-> `EXE/wwwroot/login.html` + `POST /api/auth/login`.
- `Register UI mẫu` <-> `EXE/wwwroot/register.html` + `POST /api/auth/register`.
- `Landing page mẫu` <-> `EXE/wwwroot/index.html` (trang giới thiệu sản phẩm).
- `AI tạo mô tả sản phẩm` <-> `EXE/wwwroot/app/ai.html` + `POST /api/ai/generate` (kết hợp upload ảnh).
- `Tạo/chỉnh bài viết + preview` <-> `EXE/wwwroot/app/posts.html` + `GET/POST/PUT /api/posts/*`.
- `Timeline xuất bản` <-> `EXE/wwwroot/app/schedules.html` + `GET/POST/PUT/DELETE /api/schedules/*`.
- `Dashboard tổng quan` <-> `EXE/wwwroot/app/dashboard.html` + `GET /api/dashboard/stats`.

### (b) Chức năng có sẵn nhưng KHÔNG có mẫu UI rõ ràng trong bộ ảnh

- `Plans/subscription` (`/api/plans*`, `/app/plans.html`) chỉ thấy banner nâng cấp, chưa có màn quản lý gói đầy đủ.
- `Credits` và `Payments` (`/api/credits*`, `/api/payments*`) chưa có màn ví/giao dịch tương ứng trong ảnh mẫu.
- `Affiliate` (`/api/affiliates*`) chưa có màn tương ứng trong ảnh mẫu.
- `Notifications center` (`/api/notifications*`) chưa có màn danh sách thông báo tương ứng.
- `Media library độc lập` (`/api/media*`) chưa có màn thư viện media riêng đúng module.
- `Kết nối kênh Facebook/WordPress/Zalo` (`/api/facebook*`, `/api/wordpress*`, `/api/zalo*`) chưa có màn cấu hình kết nối chi tiết.
- `Admin + Users management` (`/api/admin*`, `/api/users*`) chưa thấy UI quản trị hệ thống tương ứng.
- `Forgot/Reset password flow` (`/api/auth/forgot-password`, `/api/auth/reset-password`) chưa có màn reset riêng hoàn chỉnh.

### (c) Mẫu UI có nhưng dự án `EXE` chưa có chức năng backend tương ứng

- `Brand Memory` module đầy đủ (hồ sơ doanh nghiệp, kênh bán, sản phẩm, nhận diện thương hiệu, tone, hashtag) hiện chưa có API/module chuyên biệt tương ứng trong backend.
- `Workspace cộng tác` kiểu `Thành viên`, `Nhóm của tôi`, `Lĩnh vực, dự án` theo flow người dùng cuối chưa có contract API rõ trong backend hiện tại.
- `Dashboard quản lý công việc cá nhân` (giờ làm, tiến độ task, lịch công việc) là domain khác với các endpoint hiện có.
- `Đăng nhập/đăng ký social (Google/Facebook)` chưa có endpoint OAuth tương ứng trong `AuthController`.
- `Thêm thành viên` theo workspace/campaign chưa thấy API tương ứng trong phần user-facing workflow.

## 5) Ghi chú để sang Bước 3

- Có thể refactor frontend `EXE` theo phong cách FlowMate cho các module tương ứng ở mục (a) mà không cần đổi backend contract.
- Các mục (c) không nên tự ý thêm backend mới; cần bạn xác nhận phạm vi:
  - chỉ làm UI placeholder theo style chung, hoặc
  - mở rộng backend trong pha sau (nếu bạn yêu cầu riêng).
