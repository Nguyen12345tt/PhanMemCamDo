using System.ComponentModel.DataAnnotations;
using PhanMemCamDo.Models.Enums;

namespace PhanMemCamDo.Models.ViewModels
{
    public class RegisterVM
    {
        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        public string ?Username { get; set; }

        [Required(ErrorMessage = "Họ tên không được để trống")]
        public string ?FullName { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [DataType(DataType.Password)]
        public string ?Password { get; set; }

        public string ?Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        public string ?PhoneNumber { get; set; }

        [Required(ErrorMessage = "Phải nhập lại mật khẩu")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Mật khẩu nhập lại không khớp")] // Tự động so sánh
        public string ?ConfirmPassword { get; set; }

        [Display(Name = "Vai Trò / Quyền Hạn")]
        public UserRole Role { get; set; } = UserRole.Staff;


    }
}