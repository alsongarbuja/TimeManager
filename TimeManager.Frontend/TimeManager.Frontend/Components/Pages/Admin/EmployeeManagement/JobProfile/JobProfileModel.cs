using System.ComponentModel.DataAnnotations;
using TimeManager.Frontend.Components.Pages.Admin.EmployeeManagement.Employee;
using TimeManager.Frontend.Components.Pages.Admin.EmployeeManagement.ProfileTemplate;

namespace TimeManager.Frontend.Components.Pages.Admin.EmployeeManagement.JobProfile
{
    public class JobProfileModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Profile template Id is required")]
        public int ProfileTemplateId { get; set; }

        [Required(ErrorMessage = "Employee Id is required")]
        public int EmployeeId { get; set; }

        public string EmployeeString { get; set; } = string.Empty;
        public string ProfileTemplateString { get; set; } = string.Empty;
    }
}
