using System.ComponentModel.DataAnnotations;

namespace PhanMemCamDo.Models.Entities
{
    public class ActionLog
    {
        [Key]
        public int Id { get; set; }
        public string ?ActionName { get; set; }   // VD: "XÓA HỢP ĐỒNG", "TẠO MỚI", "SỬA"
        public string ?EntityName { get; set; }   // VD: "PawnContract", "Asset"
        public string ?Description { get; set; }  // Chi tiết: "Xóa HĐ mã HD123..."
        public DateTime Timestamp { get; set; } = DateTime.Now; // Thời gian thực hiện
        public string UserName { get; set; } = "Admin"; // Tạm thời để cứng, sau này có Login thì lấy tên User thật
    }
}