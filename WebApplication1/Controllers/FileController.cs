using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebApplication1.DataAccess;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class FileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public FileController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // 1. GET: Trang danh sách các file đã upload (Có tìm kiếm, lọc định dạng và phân trang)
        public async Task<IActionResult> Index(string? filter, string? search, string? fileType, int page = 1)
        {
            var query = _context.StoredFiles.AsQueryable();

            // Lọc theo "My Files" (Tài liệu của tôi)
            if (filter == "my")
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (currentUserId != null)
                {
                    query = query.Where(f => f.UserId == currentUserId);
                }
                else
                {
                    return RedirectToPage("/Account/Login", new { area = "Identity" });
                }
            }

            // Tìm kiếm theo tên/mô tả/tên file gốc
            if (!string.IsNullOrEmpty(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(f => f.Title.ToLower().Contains(searchLower) || 
                                         (f.Description != null && f.Description.ToLower().Contains(searchLower)) ||
                                         f.OriginalFileName.ToLower().Contains(searchLower));
            }

            // Lọc theo định dạng file
            if (!string.IsNullOrEmpty(fileType) && fileType != "all")
            {
                var ft = fileType.ToLower();
                switch (ft)
                {
                    case "pdf":
                        query = query.Where(f => f.OriginalFileName.ToLower().EndsWith(".pdf"));
                        break;
                    case "word":
                        query = query.Where(f => f.OriginalFileName.ToLower().EndsWith(".doc") || f.OriginalFileName.ToLower().EndsWith(".docx"));
                        break;
                    case "excel":
                        query = query.Where(f => f.OriginalFileName.ToLower().EndsWith(".xls") || f.OriginalFileName.ToLower().EndsWith(".xlsx"));
                        break;
                    case "ppt":
                        query = query.Where(f => f.OriginalFileName.ToLower().EndsWith(".ppt") || f.OriginalFileName.ToLower().EndsWith(".pptx"));
                        break;
                    case "image":
                        query = query.Where(f => f.OriginalFileName.ToLower().EndsWith(".png") || f.OriginalFileName.ToLower().EndsWith(".jpg") || f.OriginalFileName.ToLower().EndsWith(".jpeg") || f.OriginalFileName.ToLower().EndsWith(".gif") || f.OriginalFileName.ToLower().EndsWith(".svg") || f.OriginalFileName.ToLower().EndsWith(".webp"));
                        break;
                    case "video":
                        query = query.Where(f => f.OriginalFileName.ToLower().EndsWith(".mp4") || f.OriginalFileName.ToLower().EndsWith(".mov") || f.OriginalFileName.ToLower().EndsWith(".avi") || f.OriginalFileName.ToLower().EndsWith(".mkv"));
                        break;
                    case "audio":
                        query = query.Where(f => f.OriginalFileName.ToLower().EndsWith(".mp3") || f.OriginalFileName.ToLower().EndsWith(".wav") || f.OriginalFileName.ToLower().EndsWith(".ogg") || f.OriginalFileName.ToLower().EndsWith(".m4a"));
                        break;
                    case "archive":
                        query = query.Where(f => f.OriginalFileName.ToLower().EndsWith(".zip") || f.OriginalFileName.ToLower().EndsWith(".rar") || f.OriginalFileName.ToLower().EndsWith(".7z") || f.OriginalFileName.ToLower().EndsWith(".tar") || f.OriginalFileName.ToLower().EndsWith(".gz"));
                        break;
                    case "text":
                        query = query.Where(f => f.OriginalFileName.ToLower().EndsWith(".txt") || f.OriginalFileName.ToLower().EndsWith(".md"));
                        break;
                    case "other":
                        query = query.Where(f => 
                            !f.OriginalFileName.ToLower().EndsWith(".pdf") &&
                            !f.OriginalFileName.ToLower().EndsWith(".doc") && !f.OriginalFileName.ToLower().EndsWith(".docx") &&
                            !f.OriginalFileName.ToLower().EndsWith(".xls") && !f.OriginalFileName.ToLower().EndsWith(".xlsx") &&
                            !f.OriginalFileName.ToLower().EndsWith(".ppt") && !f.OriginalFileName.ToLower().EndsWith(".pptx") &&
                            !f.OriginalFileName.ToLower().EndsWith(".png") && !f.OriginalFileName.ToLower().EndsWith(".jpg") && !f.OriginalFileName.ToLower().EndsWith(".jpeg") && !f.OriginalFileName.ToLower().EndsWith(".gif") && !f.OriginalFileName.ToLower().EndsWith(".svg") && !f.OriginalFileName.ToLower().EndsWith(".webp") &&
                            !f.OriginalFileName.ToLower().EndsWith(".mp4") && !f.OriginalFileName.ToLower().EndsWith(".mov") && !f.OriginalFileName.ToLower().EndsWith(".avi") && !f.OriginalFileName.ToLower().EndsWith(".mkv") &&
                            !f.OriginalFileName.ToLower().EndsWith(".mp3") && !f.OriginalFileName.ToLower().EndsWith(".wav") && !f.OriginalFileName.ToLower().EndsWith(".ogg") && !f.OriginalFileName.ToLower().EndsWith(".m4a") &&
                            !f.OriginalFileName.ToLower().EndsWith(".zip") && !f.OriginalFileName.ToLower().EndsWith(".rar") && !f.OriginalFileName.ToLower().EndsWith(".7z") && !f.OriginalFileName.ToLower().EndsWith(".tar") && !f.OriginalFileName.ToLower().EndsWith(".gz") &&
                            !f.OriginalFileName.ToLower().EndsWith(".txt") && !f.OriginalFileName.ToLower().EndsWith(".md"));
                        break;
                }
            }

            // Phân trang
            int pageSize = 9;
            int totalFiles = await query.CountAsync();
            
            if (page < 1) page = 1;
            int totalPages = (int)Math.Ceiling(totalFiles / (double)pageSize);
            if (totalPages > 0 && page > totalPages) page = totalPages;

            var files = await query.OrderByDescending(f => f.UploadedAt)
                                   .Skip((page - 1) * pageSize)
                                   .Take(pageSize)
                                   .ToListAsync();
            
            ViewBag.CurrentFilter = filter ?? "all";
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentFileType = fileType ?? "all";
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages == 0 ? 1 : totalPages;
            ViewBag.TotalFiles = totalFiles;

            return View(files);
        }

        // 2. GET: Hiển thị giao diện Form để chọn file
        [Authorize]
        public IActionResult Upload()
        {
            return View();
        }

        // 3. POST: Xử lý file gửi từ Client lên Server
        [HttpPost]
        [Authorize] // <-- QUAN TRỌNG: Phải khóa cổng POST để chặn đứng hacker gửi file nặc danh
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(string title, string description, IFormFile uploadFile, int expirationDays)
        {
            if (uploadFile == null || uploadFile.Length == 0)
            {
                ModelState.AddModelError("", "Vui lòng chọn một file để tải lên.");
                return View();
            }

            var extension = Path.GetExtension(uploadFile.FileName).ToLowerInvariant();
            var forbiddenExtensions = new[] { ".exe", ".msi", ".sh", ".php", ".asp", ".aspx" };
            if (forbiddenExtensions.Contains(extension))
            {
                ModelState.AddModelError("", "Định dạng file nguy hiểm, không được phép tải lên!");
                return View();
            }

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(uploadFile.FileName);
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await uploadFile.CopyToAsync(stream);
            }

            var fileModel = new StoredFile
            {
                Title = string.IsNullOrEmpty(title) ? uploadFile.FileName : title,
                Description = description,
                OriginalFileName = uploadFile.FileName,
                StorageFileName = uniqueFileName,
                FileSize = uploadFile.Length,
                ContentType = uploadFile.ContentType,
                UploadedAt = DateTime.Now,
                LastDownloadedAt = DateTime.Now, // Khởi tạo bằng thời gian tải lên
                InactivityExpirationDays = expirationDays, // Gán số ngày tự động xóa
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier) // Chắc chắn không bị null vì đã có [Authorize] ở trên
            };

            _context.StoredFiles.Add(fileModel);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // 4. GET: Tải file về máy an toàn
        public async Task<IActionResult> Download(int id)
        {
            var file = await _context.StoredFiles.FindAsync(id);
            if (file == null) return NotFound();

            var filePath = Path.Combine(_environment.WebRootPath, "uploads", file.StorageFileName);
            if (!System.IO.File.Exists(filePath)) return NotFound("File không tồn tại trên hệ thống vật lý.");

            // Tăng số lượt tải xuống và cập nhật thời điểm tải cuối cùng
            file.DownloadCount++;
            file.LastDownloadedAt = DateTime.Now;
            _context.StoredFiles.Update(file);
            await _context.SaveChangesAsync();

            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            return File(fileBytes, file.ContentType, file.OriginalFileName);
        }

        // 5. GET: Hiển thị trang xác nhận xóa file
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var file = await _context.StoredFiles.FirstOrDefaultAsync(m => m.Id == id);
            if (file == null) return NotFound();

            // --- BẢO MẬT: Kiểm tra xem User hiện tại có phải chủ sở hữu file không ---
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (file.UserId != currentUserId)
            {
                return Forbid(); // Trả về lỗi 403 Forbidden (Không có quyền truy cập)
            }

            return View(file);
        }

        // 6. POST: Xử lý xóa file thực tế khỏi DB và Ổ đĩa
        [HttpPost, ActionName("Delete")]
        [Authorize] // <-- QUAN TRỌNG: Phải khóa bảo mật cả cổng POST thực thi xóa
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var file = await _context.StoredFiles.FindAsync(id);
            if (file == null) return NotFound();

            // --- BẢO MẬT: Chặn đứng hành vi dùng tool/Postman gán ID để xóa lậu file người khác ---
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (file.UserId != currentUserId)
            {
                return Forbid();
            }

            // --- XÓA FILE VẬT LÝ TRÊN Ổ ĐĨA ---
            var filePath = Path.Combine(_environment.WebRootPath, "uploads", file.StorageFileName);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            // --- XÓA THÔNG TIN TRONG DATABASE ---
            _context.StoredFiles.Remove(file);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}