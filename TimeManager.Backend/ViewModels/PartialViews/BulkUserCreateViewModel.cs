using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace TimeManager.Backend.ViewModels.PartialViews
{
    public class BulkUserCreateViewModel
    {
        [Required(ErrorMessage = "File is required")]
        [Display(Name = "File")]
        public IFormFile ExcelFile { get; set; }

        [Display(Name = "Department")]
        public int? DepartmentId { get; set; }

        public IEnumerable<SelectListItem> Departments { get; set; } = [];
    }
}
