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

        // 1. GET: Trang danh sách các file đã upload (Có tìm kiếm và lọc)
        public async Task<IActionResult> Index(string? filter, string? search)
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

            // Tìm kiếm theo tên/mô tả
            if (!string.IsNullOrEmpty(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(f => f.Title.ToLower().Contains(searchLower) || 
                                         (f.Description != null && f.Description.ToLower().Contains(searchLower)) ||
                                         f.OriginalFileName.ToLower().Contains(searchLower));
            }

            var files = await query.OrderByDescending(f => f.UploadedAt).ToListAsync();
            
            ViewBag.CurrentFilter = filter ?? "all";
            ViewBag.CurrentSearch = search;

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
        public async Task<IActionResult> Upload(string title, string description, IFormFile uploadFile)
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

            // Tăng số lượt tải xuống
            file.DownloadCount++;
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