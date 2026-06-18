using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace TimeManager.Backend.ViewModels
{
    public class JobProfileViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Profile template Id is required")]
        public int ProfileTemplateId { get; set; }

        [Required(ErrorMessage = "Employee Id is required")]
        public int EmployeeId { get; set; }

        public string ProfileTemplateString { get; set; } = string.Empty;
        public string EmployeeString { get; set; } = string.Empty;

        public IEnumerable<SelectListItem> ProfileTemplates { get; set; } = [];
        public IEnumerable<SelectListItem> Employees { get; set; } = [];
    }
}
