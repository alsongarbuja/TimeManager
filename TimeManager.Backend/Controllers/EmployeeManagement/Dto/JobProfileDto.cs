using System.ComponentModel.DataAnnotations;

namespace TimeManager.Backend.Controllers.EmployeeManagement.Dto
{
    public class JobProfileDto
    {
        [Required(ErrorMessage = "Profile template Id is required")]
        public int ProfileTemplateId { get; set; }

        [Required(ErrorMessage = "Employee Id is required")]
        public int EmployeeId { get; set; }
    }
}
