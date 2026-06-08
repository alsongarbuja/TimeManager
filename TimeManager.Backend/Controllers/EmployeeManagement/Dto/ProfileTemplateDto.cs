using System.ComponentModel.DataAnnotations;

namespace TimeManager.Backend.Controllers.EmployeeManagement.Dto
{
    public class ProfileTemplateDto
    {
        [Required(ErrorMessage = "Unit Id is required")]
        public int UnitId { get; set; }

        [Required(ErrorMessage = "Employee Type Id is required")]
        public int EmployeeTypeId { get; set; }
        
        [Required(ErrorMessage = "Pay Frequency Id is required")]
        public int PayFrequencyId { get; set; }

        [Required(ErrorMessage = "Role Id is required")]
        public int RoleId { get; set; }

        [Required(ErrorMessage = "Shift start time is required")]
        public TimeOnly ShiftStartTime { get; set; }

        public int EarlyClockInBufferMin { get; set; } = 5;
    }
}
