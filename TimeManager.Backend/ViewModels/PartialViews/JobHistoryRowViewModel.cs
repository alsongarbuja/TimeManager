using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace TimeManager.Backend.ViewModels.PartialViews
{
    public class JobHistoryRowViewModel
    {
        public int? Id { get; set; }

        public int JobProfileId { get; set; }

        [Required]
        [Display(Name = "Profile Group")]
        public int ProfileTemplateId { get; set; }
        public IEnumerable<SelectListItem> ProfileTemplates { get; set; } = [];

        [Required]
        [Display(Name = "Join Date")]
        public DateTime JoinDate { get; set; }

        [Display(Name = "End Date")]
        public DateTime? EndDate { get; set; }
    }
}
