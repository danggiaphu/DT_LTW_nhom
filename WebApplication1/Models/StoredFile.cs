using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class StoredFile
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tiêu đề file")]
        [StringLength(255)]
        public string Title { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public string StorageFileName { get; set; } // Tên file đã mã hóa lưu trên ổ đĩa (GUID_filename.ext)

        [Required]
        public string OriginalFileName { get; set; } // Tên gốc của file để user tải về

        public long FileSize { get; set; } // Kích thước bằng Bytes

        [StringLength(100)]
        public string ContentType { get; set; } // Định dạng file (MIME type)

        public DateTime UploadedAt { get; set; } = DateTime.Now;

        public string? UserId { get; set; } // Khóa ngoại kết nối với bảng User sau này

        public int DownloadCount { get; set; } = 0; // Số lượt tải xuống tài liệu

        public DateTime? LastDownloadedAt { get; set; } // Thời điểm tải xuống gần nhất

        public int InactivityExpirationDays { get; set; } = 30; // Số ngày tự động xóa nếu không hoạt động
    }
}

