using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class FileShareLink
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int FileId { get; set; } // Liên kết tới file gốc

        [Required]
        public string ShareToken { get; set; } // Chuỗi ngẫu nhiên duy nhất trên URL (Ví dụ: abcd-1234-xyz)

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Cấu hình thời gian hết hạn (Nếu NULL tức là vô thời hạn)
        public DateTime? ExpiresAt { get; set; }

        [ForeignKey("FileId")]
        public virtual StoredFile? StoredFile { get; set; }

        // Kiểm tra xem link này còn hiệu lực không
        public bool IsExpired => ExpiresAt.HasValue && ExpiresAt.Value < DateTime.Now;
    }
}
