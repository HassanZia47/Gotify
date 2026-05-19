using System.ComponentModel.DataAnnotations;

namespace Goatify.Core.Models
{
    public class Breeding: BaseAuditEntity
    {
        public int Id { get; set; }
        [Required]
        public int FemaleGoatId { get; set; }
        [Required]
        public int MaleGoatId { get; set; }
        [Required]
        public DateTime? BreedingDate { get; set; }
        [Required]
        public DateTime? ExpectedDeliveryDate { get; set; } = DateTime.Now.AddDays(150);
        public DateTime? ActualDeliveryDate { get; set; }
        public int? KidsBorn { get; set; }
        public string? Notes { get; set; }
    }
}
