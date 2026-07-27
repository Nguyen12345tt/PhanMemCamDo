using System.ComponentModel.DataAnnotations;
using PhanMemCamDo.Models.Enums; // Để lấy database InterestType

namespace PhanMemCamDo.Models.ViewModels
{
    public class ContractInputVM
    {
        // -- Thông tin khách (Chỉ nhập cái cần thiết) ---
        [Required(ErrorMessage = "Phải nhập tên khách")]
        public string ?CustomerName { get; set; }

        [Required(ErrorMessage = "Phải có CCCD")]
        public string ?IdentityCard { get; set; }
        public string ?PhoneNumber { get; set; }

        // --- Thông tin đồ ---
        [Required(ErrorMessage = "Phải nhập tên món đồ")]
        public string ?AssetName { get; set; } // VD: iPhone 15

        // --- Thông tin tiền nong ---
        [Required]
        [Range(100000, double.MaxValue, ErrorMessage = "Cầm ít nhất 100k")]
        public decimal PawnAmount { get; set; }

        [Required]
        public InterestType InterestType { get; set; } // Chọn kiểu tính lãi (Ngày/Tuần/Tháng)

        // Lãi suất (Nhập tay hoặc tự động)
        public decimal InterestRate { get; set; }

        // --- LƯU Ý ---
        // Không hề có Id (Tự sinh)
        // Không hề có Status (Mặc định là Active)
        // Không hề có CreatedDate (Mặc định là Now)
        // -> Code cực kỳ gọn và an toàn.
    }
}