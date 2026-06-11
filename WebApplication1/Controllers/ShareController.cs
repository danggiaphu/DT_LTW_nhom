using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebApplication1.DataAccess;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class ShareController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public ShareController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // 1. POST: Tạo link chia sẻ mới (Chỉ chủ sở hữu file mới được tạo)
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateLink(int fileId, int? expireHours)
        {
            var file = await _context.StoredFiles.FindAsync(fileId);
            if (file == null) return NotFound();

            // Bảo mật: Check xem có đúng chủ file không
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (file.UserId != currentUserId) return Forbid();

            // Tạo Token ngẫu nhiên (sử dụng GUID để không bị đoán trước)
            string token = Guid.NewGuid().ToString("N").Substring(0, 12);

            var fileShare = new FileShareLink
            {
                FileId = fileId,
                ShareToken = token,
                CreatedAt = DateTime.Now,
                // Nếu người dùng chọn thời gian thì cộng thêm giờ, ngược lại để null (vĩnh viễn)
                ExpiresAt = expireHours.HasValue ? DateTime.Now.AddHours(expireHours.Value) : null
            };

            _context.FileShareLinks.Add(fileShare);
            await _context.SaveChangesAsync();

            // Trả về dữ liệu để hiển thị link (Có thể dùng TempData hoặc lưu thông báo)
            var request = HttpContext.Request;
            string shareUrl = $"{request.Scheme}://{request.Host}/Share/Go/{token}";

            TempData["ShareUrl"] = shareUrl;
            TempData["SharedFileTitle"] = file.Title;

            return RedirectToAction("Index", "File");
        }

        // 2. GET: Tiếp nhận Link chia sẻ từ người dùng vãng lai (Public - Không cần [Authorize])
        [Route("Share/Go/{token}")]
        public async Task<IActionResult> Go(string token)
        {
            if (string.IsNullOrEmpty(token)) return NotFound();

            // Tìm mã token kèm thông tin file
            var shareInfo = await _context.FileShareLinks
                .Include(s => s.StoredFile)
                .FirstOrDefaultAsync(s => s.ShareToken == token);

            if (shareInfo == null) return NotFound("Liên kết chia sẻ không tồn tại.");

            // KIỂM TRA THỜI GIAN HẾT HẠN
            if (shareInfo.IsExpired)
            {
                return View("LinkExpired"); // Trả về view báo link đã vô hiệu hóa
            }

            return View(shareInfo);
        }

        // 3. GET: Thực hiện tải file xuống từ link public
        public async Task<IActionResult> DownloadPublic(string token)
        {
            var shareInfo = await _context.FileShareLinks
                .Include(s => s.StoredFile)
                .FirstOrDefaultAsync(s => s.ShareToken == token);

            if (shareInfo == null || shareInfo.IsExpired) return NotFound("Liên kết đã hết hạn hoặc không tồn tại.");

            var file = shareInfo.StoredFile;
            var filePath = Path.Combine(_environment.WebRootPath, "uploads", file.StorageFileName);

            if (!System.IO.File.Exists(filePath)) return NotFound("File gốc đã bị xóa.");

            // Tăng số lượt tải xuống và cập nhật thời gian tải cuối
            file.DownloadCount++;
            file.LastDownloadedAt = DateTime.Now;
            _context.StoredFiles.Update(file);
            await _context.SaveChangesAsync();

            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            return File(fileBytes, file.ContentType, file.OriginalFileName);
        }
    }
}