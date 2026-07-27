using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PhanMemCamDo.Models.Enums;

namespace PhanMemCamDo.Models.Entities
{
    [Table("Users")]
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string ?Username { get; set; }

        [Required]
        //public string ?PasswordHash { get; set; } // Mật khẩu (đã mã hóa)
        public string ?Password { get; set; } // Mật khẩu (chưa mã hóa) - Chỉ dùng tạm trong giai đoạn đầu
        
        [Required]
        public string ?FullName { get; set; }

        [Required]
        public string ?Email { get; set; }

        [Required]
        [StringLength(12)]
        public string ?PhoneNumber { get; set; }

        public UserRole Role { get; set; } = UserRole.Staff; // Mặc định là nhân viên
        public bool IsActive { get; set; } = true; // Còn làm việc hay đã nghỉ
    }
}