using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Goatify.Core.Models
{
    public class Expense : BaseAuditEntity
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        [Range(0.01, double.MaxValue,
        ErrorMessage = "Quantity must be greater than 0")]
        public decimal Quantity { get; set; }
        public string? Unit { get; set; }
        [Required]
        [Range(0.01, double.MaxValue,
        ErrorMessage = "Price Per Unit must be greater than 0")]
        public decimal PricePerUnit { get; set; }
        [Required]
        public DateTime? Date { get; set; }
        public string? Notes { get; set; }
        [NotMapped]
        public decimal TotalPrice => Quantity * PricePerUnit;
    }
}
