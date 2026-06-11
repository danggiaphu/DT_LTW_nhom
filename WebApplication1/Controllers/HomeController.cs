using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.DataAccess;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Thống kê số lượng
            ViewBag.TotalFiles = await _context.StoredFiles.CountAsync();
            ViewBag.TotalDownloads = await _context.StoredFiles.SumAsync(f => f.DownloadCount);
            ViewBag.TotalUsers = await _context.Users.CountAsync();

            // Lấy 4 file mới nhất
            var recentFiles = await _context.StoredFiles
                .OrderByDescending(f => f.UploadedAt)
                .Take(4)
                .ToListAsync();

            return View(recentFiles);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
