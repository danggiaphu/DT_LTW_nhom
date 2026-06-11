using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Data;
using WebApplication1.DataAccess;
using WebApplication1.Models;

namespace WebApplication1.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public AccountController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        /// <summary>
        /// Chặn các truy cập đã đăng nhập nhưng không phải admin và chuyển hướng họ về trang đăng nhập.
        /// </summary>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!User.IsInRole("admin"))
            {
                context.Result = new RedirectResult("/Identity/Account/Login");
            }
            base.OnActionExecuting(context);
        }

        // GET: Admin/Account
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            
            // Đếm số lượng file đã đăng của từng user
            var fileCounts = await _context.StoredFiles
                .GroupBy(f => f.UserId)
                .Select(g => new { UserId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.UserId ?? "", x => x.Count);

            var userList = new List<UserAccountViewModel>();
            foreach (var user in users)
            {
                fileCounts.TryGetValue(user.Id, out int fileCount);
                
                // Lấy vai trò của user
                var roles = await _userManager.GetRolesAsync(user);
                var roleStr = string.Join(", ", roles);

                userList.Add(new UserAccountViewModel
                {
                    Id = user.Id,
                    Email = user.Email ?? "",
                    FullName = user.FullName ?? "",
                    Role = roleStr,
                    FileCount = fileCount,
                    IsDisabled = user.IsDisabled
                });
            }

            return View(userList);
        }

        // POST: Admin/Account/ToggleStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Ngăn chặn admin tự vô hiệu hóa chính mình
            var currentUserId = _userManager.GetUserId(User);
            if (user.Id == currentUserId)
            {
                TempData["ErrorMessage"] = "Bạn không thể tự vô hiệu hóa tài khoản của chính mình.";
                return RedirectToAction(nameof(Index));
            }

            user.IsDisabled = !user.IsDisabled;
            
            // Cập nhật security stamp để force logout nếu tài khoản bị vô hiệu hóa
            if (user.IsDisabled)
            {
                await _userManager.UpdateSecurityStampAsync(user);
            }

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"Đã {(user.IsDisabled ? "vô hiệu hóa" : "hủy vô hiệu hóa")} tài khoản {user.Email} thành công.";
            }
            else
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi cập nhật trạng thái tài khoản.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
