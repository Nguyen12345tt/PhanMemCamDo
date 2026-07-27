using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PhanMemCamDo.Models.Entities
{
    [Table("Customers")]
    public class Customer
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Họ và tên không được để trống")]
        [MaxLength(100, ErrorMessage = "Tên không được vượt quá 100 ký tự")]
        [Display(Name = "Họ và Tên")]
        public string? FullName { get; set; }

        [Required(ErrorMessage = "Số CCCD là bắt buộc")]
        [RegularExpression(@"^\d{12}$", ErrorMessage = "Số CCCD phải bao gồm đúng 12 chữ số")]
        [Display(Name = "Số CCCD")]
        public string? IdentityCard { get; set; }

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Số điện thoại phải bao gồm đúng 10 chữ số")]
        [Display(Name = "Số Điện Thoại")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Địa Chỉ")]
        public string? Address { get; set; }

        public List<PawnContract> PawnContracts { get; set; } = [];
    }
}