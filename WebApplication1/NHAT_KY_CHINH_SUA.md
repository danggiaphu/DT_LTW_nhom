# NHẬT KÝ VÀ TÀI LIỆU CẬP NHẬT HỆ THỐNG
**Dự án: WebApplication1 (Quản lý & Chia sẻ File trực tuyến - ASP.NET Core 8.0 MVC)**

Tài liệu này ghi lại toàn bộ các chỉnh sửa, nâng cấp và cải tiến đã thực hiện đối với dự án **WebApplication1** nhằm giúp lập trình viên tiếp quản hệ thống dễ dàng nắm bắt cấu trúc và logic nghiệp vụ mới.

---

## 1. CÁC THAY ĐỔI CHI TIẾT THEO TỪNG THÀNH PHẦN

### A. CƠ SỞ DỮ LIỆU & MODEL (`Models/StoredFile.cs`)
Bảng `StoredFiles` lưu trữ siêu dữ liệu của các file được upload. Chúng tôi đã bổ sung các trường dữ liệu sau để phục vụ tính năng đếm lượt tải và dọn dẹp tự động:
- **`DownloadCount` (int)**: Số lượt tải xuống tài liệu (mặc định = 0).
- **`LastDownloadedAt` (DateTime?)**: Ghi nhận thời điểm cuối cùng tệp tin được tải xuống. Khi tệp được tải lên lần đầu, giá trị này được khởi tạo bằng `DateTime.Now` (thời điểm tải lên).
- **`InactivityExpirationDays` (int)**: Số ngày giới hạn tệp tin không hoạt động. Nếu một tệp không có lượt tải mới trong khoảng thời gian này, hệ thống sẽ tự động xóa (mặc định = 30 ngày).

*Lưu ý:* Các migrations liên quan đã được tạo bao gồm:
1. `AddDownloadCountToStoredFile` (Thêm cột `DownloadCount`).
2. `AddLastDownloadedAtAndExpirationDays` (Thêm cột `LastDownloadedAt` và `InactivityExpirationDays`).
*Vui lòng xem file [MIGRATION_INSTRUCTIONS.md](file:///d:/Hoctap/Detai/WebApplication1/MIGRATION_INSTRUCTIONS.md) để biết cách chạy cập nhật DB bằng .NET CLI hoặc SQL Script.*

---

### B. LOGIC ĐIỀU KHIỂN & BẢO MẬT (`Controllers/FileController.cs`)
Chúng tôi đã bổ sung và thắt chặt bảo mật các tác vụ quản lý file:
1. **Lọc tệp tin nâng cao theo định dạng:**
   Tại action `Index`, bổ sung tham số `fileType` để người dùng có thể lọc nhanh các file theo nhóm định dạng:
   - **PDF**: kết thúc bằng `.pdf`
   - **Word**: `.doc`, `.docx`
   - **Excel**: `.xls`, `.xlsx`
   - **PowerPoint**: `.ppt`, `.pptx`
   - **Hình ảnh**: `.png`, `.jpg`, `.jpeg`, `.gif`, `.svg`, `.webp`
   - **Video**: `.mp4`, `.mov`, `.avi`, `.mkv`
   - **Audio**: `.mp3`, `.wav`, `.ogg`, `.m4a`
   - **File nén**: `.zip`, `.rar`, `.7z`, `.tar`, `.gz`
   - **File văn bản**: `.txt`, `.md`
   - **Khác**: các định dạng còn lại.
2. **Theo dõi hoạt động khi tải xuống (`Download` action):**
   Mỗi lần tải tệp, `DownloadCount` được tăng thêm 1 và `LastDownloadedAt` được cập nhật thành `DateTime.Now`. Việc này giúp làm mới thời gian hoạt động của tệp, ngăn tệp bị xóa tự động.
3. **Bảo mật và phân quyền tải lên/xóa tệp:**
   - **Upload file:** Chặn các định dạng nguy hiểm như `.exe`, `.msi`, `.sh`, `.php`, `.asp`, `.aspx` để tránh virus/web shell. Bắt buộc phải có token chống giả mạo `[ValidateAntiForgeryToken]` và kiểm tra quyền đăng nhập `[Authorize]`.
   - **Xóa file:** Bổ sung logic kiểm tra bảo mật nghiêm ngặt. Chỉ chủ sở hữu tệp (người có `UserId` trùng khớp với tài khoản hiện đang đăng nhập) mới được phép xóa. Nếu người dùng khác cố tình truy cập hoặc gửi request POST xóa tệp thông qua ID, hệ thống sẽ trả về lỗi `403 Forbidden` (`Forbid()`).

---

### C. DỊCH VỤ DỌN DẸP TỰ ĐỘNG CHẠY NỀN (`Services/FileCleanupService.cs`)
Để tối ưu hóa không gian lưu trữ và giải phóng các tệp tin "rác" không còn hoạt động, chúng tôi triển khai một dịch vụ chạy nền kế thừa từ `BackgroundService`:
- **Cơ chế hoạt động:** Quét cơ sở dữ liệu định kỳ mỗi **24 giờ**.
- **Tiêu chí hết hạn:** So sánh thời gian hiện tại (`DateTime.Now`) với thời điểm hoạt động cuối cùng của tệp (lấy `LastDownloadedAt` nếu đã từng tải xuống, ngược lại lấy `UploadedAt`). Nếu hiệu số ngày lớn hơn hoặc bằng `InactivityExpirationDays` của tệp đó, tệp được coi là hết hạn.
- **Quy trình xóa:**
  1. Xóa file vật lý trong thư mục `wwwroot/uploads/` bằng `System.IO.File.Delete`.
  2. Xóa các link chia sẻ công khai liên quan trong bảng `FileShareLinks`.
  3. Xóa thông tin bản ghi tệp trong database qua DbContext.
  4. Lưu thay đổi và ghi log hệ thống (`ILogger`).
- **Đăng ký hệ thống:** Dịch vụ được cấu hình chạy trong `Program.cs` thông qua dòng lệnh:
  `builder.Services.AddHostedService<WebApplication1.Services.FileCleanupService>();`

---

### D. THỐNG KÊ & BẢNG XẾP HẠNG (`Controllers/HomeController.cs`, `Controllers/LeaderboardController.cs`)
Bổ sung các bảng thông tin thống kê trực quan trên giao diện:
- **Trang chủ Dashboard:** Thống kê tổng số lượng tệp tin trên hệ thống, tổng lượt tải xuống, tổng số thành viên đăng ký, danh sách các tài liệu mới tải lên gần đây.
- **Bảng xếp hạng (Leaderboard):**
  - Top 10 tài liệu được tải xuống nhiều nhất (xếp hạng theo `DownloadCount`).
  - Top 10 đại sứ tích cực đóng góp nhiều tài liệu nhất cho hệ thống.

---

### E. QUẢN LÝ TÀI KHOẢN & LIÊN KẾT CHIA SẺ
- **Đăng nhập Google:** Cấu hình đăng nhập bằng Google OAuth thông qua `AddGoogle` trong `Program.cs`. Client ID và Client Secret được cấu hình động qua file `appsettings.json`.
- **Gửi Email phục hồi mật khẩu:** Tích hợp SMTP qua thư viện MailKit và MimeKit (`Services/EmailSender.cs`) để gửi email khôi phục mật khẩu hoặc xác nhận tài khoản một cách bảo mật.
- **Link chia sẻ có thời hạn (`ShareController.cs`):** Cho phép tạo link công khai tải tệp. Nếu link hết hạn (dựa trên thời gian tạo và thời hạn của link chia sẻ), người truy cập sẽ nhận được thông báo link đã hết hạn (`LinkExpired.cshtml`).

---

## 2. HƯỚNG DẪN KHI TRIỂN KHAI VÀ PHÁT TRIỂN TIẾP
1. **Thiết lập thư mục:** Đảm bảo thư mục vật lý `wwwroot/uploads` tồn tại trên máy chủ.
2. **Cấu hình Email & Google Login:** Khi chạy trên môi trường thực tế, cần điền đầy đủ thông số MailSettings (Email, Mật khẩu ứng dụng Google, SMTP Host) và Google ClientId/ClientSecret trong file `appsettings.json` (hoặc cấu hình Environment Variables).
3. **Thay đổi chu kỳ quét của dịch vụ chạy nền:** Mặc định `FileCleanupService` quét mỗi 24 giờ. Có thể điều chỉnh chu kỳ này tại hàm `ExecuteAsync` (`Task.Delay(TimeSpan.FromHours(24), stoppingToken)`).
