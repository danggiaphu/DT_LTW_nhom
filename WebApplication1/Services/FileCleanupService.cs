using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebApplication1.DataAccess;

namespace WebApplication1.Services
{
    public class FileCleanupService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<FileCleanupService> _logger;
        private readonly IWebHostEnvironment _environment;

        public FileCleanupService(IServiceProvider services, ILogger<FileCleanupService> logger, IWebHostEnvironment environment)
        {
            _services = services;
            _logger = logger;
            _environment = environment;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("File Cleanup Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("File Cleanup Service is scanning for expired files.");

                try
                {
                    await DoCleanupAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during file cleanup.");
                }

                // Chạy định kỳ mỗi 24 giờ.
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }

            _logger.LogInformation("File Cleanup Service is stopping.");
        }

        private async Task DoCleanupAsync()
        {
            using (var scope = _services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var now = DateTime.Now;

                // Lấy tất cả các file có giới hạn tự động xóa (InactivityExpirationDays > 0)
                var files = await context.StoredFiles
                    .Where(f => f.InactivityExpirationDays > 0)
                    .ToListAsync();

                // Lọc trên bộ nhớ các file hết hạn (kết hợp LastDownloadedAt và UploadedAt)
                var expiredFiles = files.Where(f => 
                {
                    var baseDate = f.LastDownloadedAt ?? f.UploadedAt;
                    return (now - baseDate).TotalDays >= f.InactivityExpirationDays;
                }).ToList();

                if (expiredFiles.Any())
                {
                    _logger.LogInformation($"Found {expiredFiles.Count} expired files to delete.");

                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");

                    foreach (var file in expiredFiles)
                    {
                        // 1. Xóa file vật lý
                        var filePath = Path.Combine(uploadsFolder, file.StorageFileName);
                        if (File.Exists(filePath))
                        {
                            try
                            {
                                File.Delete(filePath);
                                _logger.LogInformation($"Deleted physical file: {file.StorageFileName}");
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, $"Failed to delete physical file {filePath}");
                            }
                        }

                        // 2. Xóa các FileShareLinks liên quan
                        var shareLinks = await context.FileShareLinks.Where(s => s.FileId == file.Id).ToListAsync();
                        context.FileShareLinks.RemoveRange(shareLinks);

                        // 3. Xóa thông tin file khỏi database
                        context.StoredFiles.Remove(file);
                    }

                    await context.SaveChangesAsync();
                    _logger.LogInformation("Successfully completed clean up of expired files.");
                }
                else
                {
                    _logger.LogInformation("No expired files found.");
                }
            }
        }
    }
}
