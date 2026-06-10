using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.DataAccess;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class LeaderboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LeaderboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Leaderboard
        public async Task<IActionResult> Index()
        {
            // 1. Lấy top 10 file tải nhiều nhất
            var topFiles = await _context.StoredFiles
                .OrderByDescending(f => f.DownloadCount)
                .ThenByDescending(f => f.UploadedAt)
                .Take(10)
                .ToListAsync();

            // 2. Lấy top 10 người dùng tải lên nhiều file nhất
            var topUsersRaw = await _context.StoredFiles
                .Where(f => !string.IsNullOrEmpty(f.UserId))
                .GroupBy(f => f.UserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    FileCount = g.Count()
                })
                .OrderByDescending(x => x.FileCount)
                .Take(10)
                .ToListAsync();

            var userIds = topUsersRaw.Select(u => u.UserId).ToList();

            var usersInfo = await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .ToListAsync();

            var topUserModels = topUsersRaw.Select(tu =>
            {
                var user = usersInfo.FirstOrDefault(u => u.Id == tu.UserId);
                return new UserLeaderboardModel
                {
                    UserId = tu.UserId,
                    FileCount = tu.FileCount,
                    FullName = user?.FullName ?? "Người dùng ẩn danh",
                    Email = user?.Email ?? "N/A",
                    UserName = user?.UserName ?? "N/A"
                };
            }).ToList();

            var viewModel = new LeaderboardViewModel
            {
                TopFiles = topFiles,
                TopUsers = topUserModels
            };

            return View(viewModel);
        }
    }
}
