using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PhanMemCamDo.Models.Entities
{
    [Table("Notifications")]
    public class Notification
    {
        [Key]
        public int Id { get; set; }
        public string ?Title { get; set; }
        public string ?Message { get; set; }
        public bool IsRead { get; set; } = false; // Đã đọc chưa
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}