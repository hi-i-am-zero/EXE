# Checklist test sau refactor frontend

## 1) Auth

- [ ] Mở `index.html` hiển thị đúng giao diện mới, CTA hoạt động.
- [ ] Đăng nhập tại `login.html` bằng tài khoản hợp lệ.
- [ ] Đăng nhập sai mật khẩu hiển thị lỗi đúng.
- [ ] Đăng ký tại `register.html` tạo tài khoản mới thành công.
- [ ] Refresh session vẫn gọi `POST /api/auth/refresh` bình thường.
- [ ] Đăng xuất từ sidebar chuyển về `login.html`.

## 2) Dashboard

- [ ] Trang `app/dashboard.html` load được số liệu từ `GET /api/dashboard/stats`.
- [ ] Biểu đồ bài viết 7 ngày hiển thị đúng.
- [ ] Biểu đồ AI usage hiển thị đúng.
- [ ] Danh sách hoạt động gần đây hiển thị (API hoặc fallback demo).

## 3) AI Content

- [ ] Trang `app/ai.html` hiển thị đúng layout 2 panel.
- [ ] Tạo mô tả qua `POST /api/Ai/generate` thành công.
- [ ] Sao chép mô tả hoạt động.
- [ ] Tạo bài viết từ kết quả AI (`POST /api/Posts`) thành công.
- [ ] Danh sách prompt (`GET /api/Ai/prompts`) hiển thị đúng.
- [ ] Thêm prompt (`POST /api/Ai/prompts`) thành công.
- [ ] Xóa prompt cá nhân (`DELETE /api/Ai/prompts/{id}`) thành công.
- [ ] Lịch sử AI (`GET /api/Ai/history`) tải được.

## 4) Posts (Tạo bài viết đơn)

- [ ] Trang `app/posts.html` load danh sách bài viết (`GET /api/Posts`) bình thường.
- [ ] Tạo bài mới (`POST /api/Posts`) thành công.
- [ ] Cập nhật bài (`PUT /api/Posts/{id}`) thành công.
- [ ] Xóa bài (`DELETE /api/Posts/{id}`) thành công.
- [ ] Lên lịch từ composer (`POST /api/Schedules`) thành công.
- [ ] Preview nội dung cập nhật đúng theo input.

## 5) Schedules (Timeline)

- [ ] Trang `app/schedules.html` tải danh sách lịch (`GET /api/Schedules`) thành công.
- [ ] Tạo lịch mới (`POST /api/Schedules`) thành công.
- [ ] Cập nhật lịch (`PUT /api/Schedules/{id}`) thành công.
- [ ] Xóa/hủy lịch (`DELETE /api/Schedules/{id}`) thành công.
- [ ] Chọn template timeline không làm lỗi luồng tạo lịch.

## 6) Các module còn lại

- [ ] `app/channels.html` hiển thị đúng style mới, các nút demo/API không lỗi JS.
- [ ] `app/media.html` upload media vẫn hoạt động (`POST /api/Media/upload`).
- [ ] `app/notifications.html` đọc 1 thông báo (`PUT /api/Notifications/{id}/read`) thành công.
- [ ] `app/notifications.html` đọc tất cả (`PUT /api/Notifications/read-all`) thành công.
- [ ] `app/plans.html` lấy plans/subscription (`GET /api/plans`, `GET /api/plans/subscriptions`) thành công.
- [ ] `app/credits.html` load số dư và giao dịch (`GET /api/credits`, `GET /api/credits/transactions`) thành công.
- [ ] `app/affiliate.html` load profile/links/commissions thành công.
- [ ] `app/admin.html` (role Admin/SuperAdmin) load dashboard + user list thành công.

## 7) Realtime / SignalR

- [ ] Kiểm tra endpoint hub `/hubs/notifications` còn truy cập được khi backend chạy.
- [ ] Trigger thông báo từ backend và xác nhận API notifications vẫn phản ánh dữ liệu mới.
- [ ] Xác nhận không có lỗi JS liên quan selector sau thay đổi UI.

## 8) Responsive và UX

- [ ] Kiểm tra desktop (>= 1280px): sidebar/topbar/card layout đúng.
- [ ] Kiểm tra tablet (~768-1024px): layout không vỡ cột chính.
- [ ] Kiểm tra mobile (< 768px): form và nút bấm không tràn màn hình.
- [ ] Kiểm tra contrast/độ đọc chữ ở banner, button, bảng.

## 9) Build/Test đã chạy trong quá trình refactor

- [x] `dotnet build .\frontend\AutoWork.Web\AutoWork.Web.csproj` (kết quả cuối: 0 warnings, 0 errors).
- [x] `dotnet test .\tests\AutoWork.UnitTests\AutoWork.UnitTests.csproj` (Passed: 1, Failed: 0, Skipped: 0).
