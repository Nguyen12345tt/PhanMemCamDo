using System.ComponentModel.DataAnnotations;

namespace PhanMemCamDo.Models.ViewModels
{
    public class ShopConfigViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên cửa hàng")]
        [Display(Name = "Tên Cửa Hàng / Tiệm Cầm Đồ")]
        public string TenCuaHang { get; set; } = "Cầm Đồ Xịn";

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Display(Name = "Số Điện Thoại Hotline")]
        public string SoDienThoai { get; set; } = "0976543123";

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ cửa hàng")]
        [Display(Name = "Địa Chỉ Cửa Hàng")]
        public string DiaChi { get; set; } = "123 Nguyễn Trãi, Thanh Xuân, Hà Nội";

        [Required(ErrorMessage = "Vui lòng nhập lãi suất mặc định")]
        [Range(0.1, 100, ErrorMessage = "Lãi suất phải từ 0.1% đến 100%")]
        [Display(Name = "Lãi Suất Mặc Định (%/tháng)")]
        public decimal LaiSuatMacDinh { get; set; } = 3.0m;

        [Required(ErrorMessage = "Vui lòng nhập hệ số phạt")]
        [Range(1.0, 5.0, ErrorMessage = "Hệ số phạt từ 1.0 đến 5.0")]
        [Display(Name = "Hệ Số Phạt Quá Hạn")]
        public decimal HeSoPhatQuaHan { get; set; } = 1.5m;

        [Required(ErrorMessage = "Vui lòng nhập số ngày cảnh báo")]
        [Range(1, 30, ErrorMessage = "Số ngày từ 1 đến 30 ngày")]
        [Display(Name = "Số Ngày Cảnh Báo Hết Hạn (Ngày)")]
        public int ThoiHanCanhBao { get; set; } = 3;
    }
}
