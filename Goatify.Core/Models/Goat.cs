using Goatify.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace Goatify.Core.Models
{
    public class Goat : BaseAuditEntity
    {
        public int Id { get; set; }
        [Required]
        public GoatGender Gender { get; set; }
        [Required]
        public GoatStatus Status { get; set; }
        [Required]
        [Range(0.01, double.MaxValue,
        ErrorMessage = "Purchase Price must be greater than 0")]
        public decimal PurchasePrice { get; set; }
        [Range(0.01, double.MaxValue,
        ErrorMessage = "Sale Price must be greater than 0")]
        public decimal? SalePrice { get; set; }
        [Required]
        public DateTime? PurchaseDate { get; set; }
        public DateTime? SellDate { get; set; }
        public string? Notes { get; set; }
        public string? ImageUrl { get; set; }
    }
}