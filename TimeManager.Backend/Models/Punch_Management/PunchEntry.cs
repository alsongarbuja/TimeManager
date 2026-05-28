using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TimeManager.Backend.Models.Employee_Management;

namespace TimeManager.Backend.Models.Punch_Management
{
    public class PunchEntry
    {
        [Key]
        public int Id { get; set; }

        public required DateTime ClockIn { get; set; }

        public DateTime? ClockOut { get; set; }

        public required int JobProfileId { get; set; }

        [ForeignKey("JobProfileId")]
        public virtual JobProfile JobProfile { get; set; } = null!;
    }
}
