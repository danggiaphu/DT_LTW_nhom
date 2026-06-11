# Hướng dẫn cập nhật Cơ sở dữ liệu (Database Migration)

Dưới đây là các cách bạn có thể thực hiện để cập nhật cơ sở dữ liệu sau khi thêm các thuộc tính `LastDownloadedAt` và `InactivityExpirationDays` vào bảng `StoredFiles`.

## Cách 1: Sử dụng .NET CLI (Khuyên dùng)
Hãy mở terminal tại thư mục gốc của dự án `d:\Hoctap\Detai\WebApplication1` và chạy lần lượt các lệnh sau:

```bash
# 1. Tạo migration mới
dotnet ef migrations add AddLastDownloadedAtAndExpirationDays

# 2. Cập nhật vào cơ sở dữ liệu
dotnet ef database update
```

## Cách 2: Sử dụng Package Manager Console (trong Visual Studio)
Nếu bạn phát triển bằng Visual Studio, hãy mở **Tools** > **NuGet Package Manager** > **Package Manager Console** và chạy các lệnh:

```powershell
Add-Migration AddLastDownloadedAtAndExpirationDays
Update-Database
```

## Cách 3: Chạy trực tiếp Script SQL bằng SSMS (SQL Server Management Studio)
Nếu bạn kết nối trực tiếp vào Database của dự án (`WebLuuTruDataDB` trên `.\SQLEXPRESS`), hãy thực thi câu lệnh SQL sau:

```sql
-- 1. Thêm cột LastDownloadedAt (cho phép NULL, dùng để theo dõi ngày tải xuống cuối cùng)
ALTER TABLE StoredFiles ADD LastDownloadedAt datetime2(7) NULL;

-- 2. Thêm cột InactivityExpirationDays (không cho phép NULL, mặc định là 30 ngày)
ALTER TABLE StoredFiles ADD InactivityExpirationDays int NOT NULL DEFAULT 30;
```
