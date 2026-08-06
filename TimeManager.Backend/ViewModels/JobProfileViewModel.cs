using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using TimeManager.Backend.Models.Responses;
using TimeManager.Backend.ViewModels.PartialViews;

namespace TimeManager.Backend.ViewModels
{
    public class JobProfileOverall
    {
        public PagedResponse<JobProfileViewModel> JobProfiles { get; set; }
        public IEnumerable<SelectListItem> Employees { get; set; } = [];
    }

    public class JobProfileViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Profile template is required")]
        [Display(Name = "Profile Group")]
        public int ProfileTemplateId { get; set; }


        [Required(ErrorMessage = "Employee is required")]
        [Display(Name = "Employee")]
        public int EmployeeId { get; set; }

        [Display(Name = "Early clock buffer")]
        public int? EarlyBuffer { get; set; }

        [Required]
        public List<JobHistoryRowViewModel> JobHistories { get; set; } = [];

        public string ProfileTemplateString { get; set; } = string.Empty;
        public string EmployeeString { get; set; } = string.Empty;
        public TimeOnly ShiftStartTime { get; set; }

        public IEnumerable<SelectListItem> ProfileTemplates { get; set; } = [];
        public IEnumerable<SelectListItem> Employees { get; set; } = [];
    }
}
