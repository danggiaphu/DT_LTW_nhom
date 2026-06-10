using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.DataAccess
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Tạo bảng lưu trữ thông tin File
        public DbSet<StoredFile> StoredFiles { get; set; }
        public DbSet<FileShareLink> FileShareLinks { get; set; }
    }
}

