using System.ComponentModel.DataAnnotations;

namespace Goatify.Core.Models
{
    public class Bill : BaseAuditEntity
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        [Range(0.01, double.MaxValue,
        ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }
        [Required]
        public DateTime? BillDate { get; set; }
        public string? Notes { get; set; }
    }
}
