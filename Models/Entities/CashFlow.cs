using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PhanMemCamDo.Models.Enums;

namespace PhanMemCamDo.Models.Entities
{
    [Table("CashFlows")]
    public class CashFlow
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Thời Gian")]
        public DateTime Date { get; set; } = DateTime.Now; // Mặc định là thời điểm tạo bản ghi

        [Required]
        [Column(TypeName = "decimal(18, 0)")]
        [Display(Name = "Số Tiền")]
        public decimal Amount { get; set; }

        [Display(Name = "Loại Thu/Chi")]
        public CashFlowType FlowType { get; set; } // Enum: Thu (Income) hoặc Chi (Expense)

        [Display(Name = "Nội Dung")]
        public string? Description { get; set; }

        [Display(Name = "Người Thực Hiện")]
        public string? UserName { get; set; } // Lưu tên Admin/Nhân viên
    }
}