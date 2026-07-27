using System.ComponentModel.DataAnnotations;

namespace PhanMemCamDo.Models.Enums
{
    public enum PaymentType
    {
        [Display(Name = "Đóng Lãi")]
        Interest = 1,

        [Display(Name = "Trả Bớt Gốc")]
        Principal = 2,

        [Display(Name = "Chuộc Đồ (Tất Toán)")]
        Redeem = 3,

        [Display(Name = "Thanh Lý Tài Sản")]
        Liquidation = 4,

        [Display(Name = "Khác")]
        Other = 5
    }
}