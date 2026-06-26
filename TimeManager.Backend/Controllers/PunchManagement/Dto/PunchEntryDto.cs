using System.ComponentModel.DataAnnotations;

namespace TimeManager.Backend.Controllers.PunchManagement.Dto
{
    public class PunchEntryDto
    {
        [Required(ErrorMessage = "SO ID is required")]
        public string UniqueId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Department Id is required")]
        public int DepartmentId { get; set; }
    }

    public class PunchDto
    {
        public DateTime ClockIn { get; set; }
        public DateTime? ClockOut { get; set; }
    }
}
