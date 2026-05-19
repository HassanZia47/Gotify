using System.ComponentModel.DataAnnotations;

namespace Goatify.Core.Models
{
    public class Medication : BaseAuditEntity
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        public string? Type { get; set; }
        public string? Notes { get; set; }
        public List<GoatMedication> GoatMedications { get; set; } = new();
    }
}
