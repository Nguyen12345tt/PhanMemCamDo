using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PhanMemCamDo.Models.Enums; // Nhớ dòng này để nhận diện PaymentType

namespace PhanMemCamDo.Models.Entities
{
    [Table("PaymentHistories")]
    public class PaymentHistory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 0)")] // Lưu số tiền Việt (không lấy số lẻ thập phân)
        [Display(Name = "Số Tiền Thu")]
        public decimal Amount { get; set; }

        [Display(Name = "Ngày Thu")]
        public DateTime PaymentDate { get; set; } = DateTime.Now;

        // 👇 CỘT QUAN TRỌNG: Phân biệt Đóng lãi / Trả gốc / Chuộc đồ
        [Display(Name = "Loại Giao Dịch")]
        public PaymentType PaymentType { get; set; } = PaymentType.Interest;

        [Display(Name = "Ghi Chú")]
        public string? Note { get; set; }

        // --- LIÊN KẾT VỚI HỢP ĐỒNG ---
        public int PawnContractId { get; set; }

        [ForeignKey("PawnContractId")]
        public virtual PawnContract? PawnContract { get; set; }
    }
}