using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PhanMemCamDo.Models.Entities
{
    [Table("SystemConfigs")]
    public class SystemConfig
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ?ConfigKey { get; set; }   // Mã cấu hình (VD: LaiSuat, TenTiem...)

        public string ?ConfigValue { get; set; } // Giá trị (VD: 5, Cầm Đồ A...)

        public string? Description { get; set; } // Giải thích (VD: Lãi suất mặc định %)
    }
}