# Hướng dẫn cập nhật Cơ sở dữ liệu (Database Migration)

Dưới đây là các cách bạn có thể thực hiện để cập nhật cơ sở dữ liệu sau khi sửa đổi/bổ sung các thuộc tính.

---

## Migration 1 & 2: Cập nhật StoredFiles
Cập nhật thuộc tính `LastDownloadedAt` và `InactivityExpirationDays` vào bảng `StoredFiles`.

### Cách 1: Sử dụng .NET CLI (Khuyên dùng)
Mở terminal tại thư mục gốc của dự án `d:\Hoctap\Detai\WebApplication1` và chạy:
```bash
dotnet ef migrations add AddLastDownloadedAtAndExpirationDays
dotnet ef database update
```

### Cách 2: Sử dụng Package Manager Console (Visual Studio)
```powershell
Add-Migration AddLastDownloadedAtAndExpirationDays
Update-Database
```

### Cách 3: Chạy trực tiếp Script SQL (SSMS)
Thực thi trên Database `WebLuuTruDataDB`:
```sql
ALTER TABLE StoredFiles ADD LastDownloadedAt datetime2(7) NULL;
ALTER TABLE StoredFiles ADD InactivityExpirationDays int NOT NULL DEFAULT 30;
```

---

## Migration 3: Thêm tính năng Vô hiệu hóa Tài khoản (IsDisabled)
Cập nhật thuộc tính `IsDisabled` vào bảng `AspNetUsers`.

### Cách 1: Sử dụng .NET CLI (Khuyên dùng)
Mở terminal tại thư mục gốc của dự án và chạy:
```bash
dotnet ef migrations add AddIsDisabledToUser
dotnet ef database update
```

### Cách 2: Sử dụng Package Manager Console (Visual Studio)
```powershell
Add-Migration AddIsDisabledToUser
Update-Database
```

### Cách 3: Chạy trực tiếp Script SQL (SSMS)
Thực thi trên Database `WebLuuTruDataDB`:
```sql
ALTER TABLE AspNetUsers ADD IsDisabled bit NOT NULL DEFAULT 0;
```

