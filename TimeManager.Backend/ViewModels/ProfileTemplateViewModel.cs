using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace TimeManager.Backend.ViewModels
{
    public class ProfileTemplateViewModel
    {
        public int Id { get; set; }
        public string Role { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public string EmployeeType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Shift start time is required"), Display(Name = "Shift start time")]
        [DataType(DataType.Time)]
        public TimeOnly ShiftStartTime { get; set; }

        [Required(ErrorMessage = "Early buffer time is required") ,Display(Name = "Early clock (min)")]
        public int EarlyClockInBufferMin { get; set; }

        public IEnumerable<SelectListItem> Units { get; set; } = [];

        [Required(ErrorMessage = "Unit is required"), Display(Name = "Unit")]
        public int UnitId { get; set; }
        public IEnumerable<SelectListItem> EmployeeTypes { get; set; } = [];

        [Required(ErrorMessage = "Employee Type is required"), Display(Name = "Employee Type")]
        public int EmployeeTypeId { get; set; }
        public IEnumerable<SelectListItem> Roles { get; set; } = [];

        [Required(ErrorMessage = "Role is required"), Display(Name = "Role")]
        public int RoleId { get; set; }
        public IEnumerable<SelectListItem> PayFrequencies { get; set; } = [];

        [Required(ErrorMessage = "Pay frequency is required"), Display(Name = "Pay Frequency")]
        public int PayFrequencyId { get; set; }

    }
}
