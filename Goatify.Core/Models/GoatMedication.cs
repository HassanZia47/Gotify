namespace Goatify.Core.Models
{
    public class GoatMedication
    {
        public int Id { get; set; }

        public int GoatId { get; set; }
        public Goat Goat { get; set; } = null!;

        public int MedicationId { get; set; }
        public Medication Medication { get; set; } = null!;

        public DateTime AssignedDate { get; set; } = DateTime.UtcNow;
    }
}
