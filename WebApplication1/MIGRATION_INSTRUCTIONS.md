# Hướng dẫn cập nhật Cơ sở dữ liệu (Database Migration)

Vì bạn yêu cầu tự thực hiện cập nhật cơ sở dữ liệu, dưới đây là các cách bạn có thể thực hiện để thêm cột `DownloadCount` vào bảng `StoredFiles`.

## Cách 1: Sử dụng .NET CLI (Khuyên dùng)
Hãy mở terminal tại thư mục gốc của dự án `d:\Hoctap\Detai\WebApplication1` và chạy lệnh sau:

```bash
dotnet ef database update
```

## Cách 2: Sử dụng Package Manager Console (trong Visual Studio)
Nếu bạn đang mở dự án bằng Visual Studio, hãy vào mục **Tools** > **NuGet Package Manager** > **Package Manager Console** và chạy lệnh:

```powershell
Update-Database
```

## Cách 3: Chạy trực tiếp Script SQL bằng SSMS
Nếu bạn quản lý cơ sở dữ liệu SQL Server trực tiếp thông qua SQL Server Management Studio (SSMS), hãy kết nối tới Database của dự án (`WebLuuTruDataDB` trên `.\SQLEXPRESS`) và thực thi câu lệnh SQL sau:

```sql
-- Thêm cột DownloadCount vào bảng StoredFiles
ALTER TABLE StoredFiles ADD DownloadCount int NOT NULL DEFAULT 0;
```
