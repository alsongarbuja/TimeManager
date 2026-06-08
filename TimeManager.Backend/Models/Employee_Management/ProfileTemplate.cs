using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TimeManager.Backend.Models.Organization_Management;

namespace TimeManager.Backend.Models.Employee_Management
{
    public class ProfileTemplate
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Employee Type is required")]
        public int EmployeeTypeId { get; set; }

        [Required(ErrorMessage = "Role is required")]
        public int RoleId { get; set; }

        [Required(ErrorMessage = "Pay Frequency is required")]
        public int PayFrequencyId { get; set; }

        [Required(ErrorMessage = "Unit is required")]
        public int UnitId { get; set; }

        [Required(ErrorMessage = "Shift start time is required")]
        public TimeOnly ShiftStartTime { get; set; }

        public int EarlyClockInBufferMin { get; set; } = 5;


        [ForeignKey("EmployeeTypeId")]
        public virtual EmployeeType EmployeeType { get; set; } = null!;

        [ForeignKey("PayFrequencyId")]
        public virtual PayFrequency PayFrequency { get; set; } = null!;

        [ForeignKey("RoleId")]
        public virtual Role Role { get; set; } = null!;

        [ForeignKey("UnitId")]
        public virtual Unit Unit { get; set; } = null!;

        public virtual ICollection<JobProfile> JobProfile { get; set; } = [];
    }
}
