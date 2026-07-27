using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PhanMemCamDo.Models.Entities
{
    [Table("Customers")]
    public class Customer
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên không được để trống")]
        [MaxLength(100)]
        public string? FullName { get; set; }

        [Required]
        [StringLength(12)]
        public string? IdentityCard { get; set; }

        [Phone]
        [StringLength(15)]
        public string? PhoneNumber { get; set; }

        public string? Address { get; set; }

        public List<PawnContract> PawnContracts { get; set; } = [];
    }
}