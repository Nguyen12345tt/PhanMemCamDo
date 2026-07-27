using System.ComponentModel.DataAnnotations;

namespace PhanMemCamDo.Models.Entities
{
    // Bảng số 2 mới: Danh mục tài sản (Thay thế cho AssetImages)
    public class AssetCategory
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Tên Loại")]
        public string ?Name { get; set; } // Ví dụ: Laptop, Xe máy, Điện thoại

        // Liên kết ngược: Một loại có nhiều tài sản
        public virtual ICollection<Asset> ?Assets { get; set; }
    }
}