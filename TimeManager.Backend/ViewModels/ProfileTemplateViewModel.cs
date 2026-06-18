using Microsoft.AspNetCore.Mvc.Rendering;

namespace TimeManager.Backend.ViewModels
{
    public class ProfileTemplateViewModel
    {
        public int Id { get; set; }
        public string Role { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public string EmployeeType { get; set; } = string.Empty;
        public TimeOnly ShiftStartTime { get; set; }
        public int EarlyClockInBufferMin { get; set; }

        public IEnumerable<SelectListItem> Units { get; set; } = [];
        public int UnitId { get; set; }
        public IEnumerable<SelectListItem> EmployeeTypes { get; set; } = [];
        public int EmployeeTypeId { get; set; }
        public IEnumerable<SelectListItem> Roles { get; set; } = [];
        public int RoleId { get; set; }
        public IEnumerable<SelectListItem> PayFrequencies { get; set; } = [];
        public int PayFrequencyId { get; set; }

    }
}
