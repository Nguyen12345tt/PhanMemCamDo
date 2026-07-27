using NuGet.ContentModel;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PhanMemCamDo.Models.Enums;

namespace PhanMemCamDo.Models.Entities
{
    [Table("PawnContracts")] // Ép tên bảng trong SQL là "PawnContracts"
    public class PawnContract
    {
        [Key]
        public int Id { get; set; }

        [StringLength(20)] // Mã hợp đồng (VD: HĐ001), không cho dài quá tốn bộ nhớ
        public string ?ContractCode { get; set; }

        // --- QUAN TRỌNG: TIỀN BẠC ---
        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        // 18 chữ số, 2 số thập phân. Đủ chứa hàng nghìn tỷ đồng mà không sai số.
        public decimal PawnAmount { get; set; }

        [Required]
        [Column(TypeName = "decimal(5, 2)")]
        // VD: Lãi suất 5.50% hoặc 100.00%
        public decimal InterestRate { get; set; }

        // --- QUẢN LÝ THỜI GIAN ---
        [Required]
        public DateTime StartDate { get; set; } = DateTime.Now; // Mặc định là lúc tạo

        [Required]
        public DateTime EndDate { get; set; } // Ngày hết hạn hợp đồng

        // --- QUẢN LÝ TRẠNG THÁI ---
        [Required]
        public ContractStatus Status { get; set; } = ContractStatus.Active;

        public string ?Note { get; set; } // Ghi chú thêm (đồ trầy xước, v.v.)

        // --- KHÓA NGOẠI (RELATIONSHIPS) ---
        // Liên kết với bảng Khách Hàng (Customer)
        public int CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public virtual Customer ?Customer { get; set; }

        // Liên kết với bảng Tài Sản (Asset)
        public int AssetId { get; set; }
        [ForeignKey("AssetId")]
        public virtual Asset ?Asset { get; set; }
        public InterestType InterestType { get; set; } // Kiểu tính lãi (Ngày/Tuần/Tháng)
    }
}