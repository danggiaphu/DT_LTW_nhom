WebApplication1
=================

Mô tả
------
WebApplication1 là một ứng dụng web ASP.NET Core (Razor Pages + MVC) xây dựng trên .NET 8. Ứng dụng cho phép người dùng đăng ký/đăng nhập (hỗ trợ đăng nhập Google, gửi mail khôi phục mật khẩu), upload/tải/xóa file, tìm kiếm và lọc file theo định dạng, phân trang danh sách và tự động giải phóng bộ nhớ bằng cách xóa tệp tin không hoạt động.

Kỹ thuật sử dụng
-----------------
- .NET 8
- ASP.NET Core Razor Pages + MVC
- Entity Framework Core (SQL Server)
- ASP.NET Core Identity cho quản lý tài khoản
- Google External Authentication (OAuth)
- MailKit & MimeKit (Gửi email qua SMTP)
- BackgroundService / HostedService (Dịch vụ chạy nền dọn dẹp file tự động)

Các chức năng chính
-------------------
1. Đăng ký / Đăng nhập local (Email + mật khẩu).
2. Đăng nhập bằng Google (External login).
3. Quên mật khẩu & Gửi link khôi phục qua Gmail (SMTP MailKit).
4. Upload file với kiểm tra định dạng nguy hiểm (.exe, .msi, v.v.), cấu hình thời hạn lưu trữ và lưu file vật lý vào wwwroot/uploads.
5. Danh sách file có phân trang (9 file/trang), tìm kiếm từ khóa và lọc nâng cao theo định dạng tệp (PDF, Word, Excel, PPT, Hình ảnh, v.v.).
6. Tải file (download) trực tiếp/công khai và đếm lượt tải kèm cập nhật thời gian hoạt động cuối của file.
7. Xóa file (chủ sở hữu được phép xóa hoặc hệ thống tự động xóa sau X ngày không hoạt động).
8. Bảng điều khiển Trang chủ (Dashboard) thống kê động (số tệp tin, lượt tải, số thành viên, và các tệp mới tải lên).
9. Bảng xếp hạng (Leaderboard) vinh danh top 10 file tải nhiều nhất và top 10 đại sứ tích cực nhất.

Cấu trúc chính của project
--------------------------
- Program.cs : cấu hình services, authentication, routing, HostedService.
- Data/ApplicationUser.cs : lớp user mở rộng (FullName).
- DataAccess/ApplicationDbContext.cs : DbContext (StoredFiles, FileShareLinks).
- Models/StoredFile.cs : model lưu metadata file, thời điểm tải cuối, hạn lưu trữ.
- Services/EmailSender.cs : xử lý gửi mail SMTP qua MailKit.
- Services/FileCleanupService.cs : background service tự động dọn dẹp file hết hạn hoạt động.
- Controllers/FileController.cs : xử lý upload, download, delete, list, lọc, phân trang.
- Controllers/ShareController.cs : quản lý tạo link chia sẻ có thời hạn và tải file public.
- Areas/Identity : các trang và logic ASP.NET Identity (login, register, forgot password, ...).

Thiết lập môi trường (chạy cục bộ)
---------------------------------
1. Sao chép project về máy và vào thư mục project:
   cd "D:\Hoctap\Detai\WebApplication1"

2. Cấu hình appsettings.json (không commit file này vào git)
   - Thêm connection string cho DefaultConnection (SQL Server).
   - Thiết lập Authentication:Google:ClientId và Authentication:Google:ClientSecret (nếu muốn bật Google login).
   - Thiết lập thông số gửi mail tại MailSettings:
     "MailSettings": {
        "Mail": "email_cua_ban@gmail.com",
        "DisplayName": "DriveShare System",
        "Password": "mat_khau_ung_dung_google_16_ky_tu",
        "Host": "smtp.gmail.com",
        "Port": 587
     }

3. Tạo thư mục uploads trong wwwroot nếu chưa tồn tại:
   mkdir wwwroot\uploads

4. Cài đặt và chạy migration (nếu chưa có database):
   dotnet tool restore
   dotnet ef database update
   # hoặc tạo migration nếu cần:
   dotnet ef migrations add InitialCreate
   dotnet ef database update

5. Chạy ứng dụng:
   dotnet run

Lưu ý khi phát triển / deploy
----------------------------
- appsettings.json được thêm vào .gitignore trong repository này. Đảm bảo cung cấp file cấu hình riêng (.env, secrets, hoặc biến môi trường) khi deploy.
- Nếu sử dụng HTTPS/Google OAuth, cấu hình lại redirect URI trong Google Cloud Console để trỏ đến URL của ứng dụng (ví dụ https://localhost:5001/signin-google).
- Kiểm soát định dạng file upload: file .exe, .msi, .sh, .php, .asp, .aspx bị cấm theo controller hiện tại.
- Hạn dọn dẹp mặc định là 30 ngày (nếu không có lượt tải mới). Thay đổi thời gian quét dọn của dịch vụ chạy nền tại Services/FileCleanupService.cs.
