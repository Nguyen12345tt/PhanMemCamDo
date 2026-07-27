using System.ComponentModel.DataAnnotations;

namespace PhanMemCamDo.Models.ViewModels
{
    public class ChangePasswordVM
    {
        // Mật khẩu cũ
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu cũ")]
        [DataType(DataType.Password)]
        public string ?OldPassword { get; set; }

        // Mật khẩu mới
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Mật khẩu mới phải có ít nhất 6 ký tự")]
        public string ?NewPassword { get; set; }

        // Nhập lại mật khẩu mới
        [Required(ErrorMessage = "Vui lòng nhập lại mật khẩu mới")]
        [DataType(DataType.Password)]
        public string ?ConfirmPassword { get; set; }
    }
}