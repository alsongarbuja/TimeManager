using System.ComponentModel.DataAnnotations;
using TimeManager.Frontend.Components.Pages.Admin.EmployeeManagement.EmployeeType;
using TimeManager.Frontend.Components.Pages.Admin.EmployeeManagement.PayFrequency;
using TimeManager.Frontend.Components.Pages.Admin.EmployeeManagement.Role;
using TimeManager.Frontend.Components.Pages.Admin.Organization.Unit;

namespace TimeManager.Frontend.Components.Pages.Admin.EmployeeManagement.ProfileTemplate
{
    public class ProfileTemplateModel
    {
        public int Id { get; set; }

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

        public UnitModel? Unit { get; set; }
        public EmployeeTypeModel? EmployeeType { get; set; }
        public RoleModel? Role { get; set; }
        public PayFrequencyModel? PayFrequency { get; set; }
    }
}
