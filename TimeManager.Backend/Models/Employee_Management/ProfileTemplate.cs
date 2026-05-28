using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TimeManager.Backend.Models.Organization_Management;

namespace TimeManager.Backend.Models.Employee_Management
{
    public class ProfileTemplate
    {
        [Key]
        public int Id { get; set; }

        public required int EmployeeTypeId { get; set; }

        public required int RoleId { get; set; }

        public required int PayFrequencyId { get; set; }

        public required int UnitId { get; set; }

        public required TimeOnly ShiftStartTime { get; set; }

        public required int EarlyClockInBufferMin { get; set; }


        [ForeignKey("EmployeeTypeId")]
        public virtual EmployeeType EmployeeType { get; set; } = null!;

        [ForeignKey("PayFrequencyId")]
        public virtual PayFrequency PayFrequency { get; set; } = null!;

        [ForeignKey("RoleId")]
        public virtual Role Role { get; set; } = null!;

        [ForeignKey("UnitId")]
        public virtual Unit Unit { get; set; } = null!;

        public virtual JobProfile? JobProfile { get; set; }
    }
}
