using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace TimeManager.Backend.ViewModels.PartialViews
{
    public class BulkJobProfileCreateViewModel
    {
        [Required(ErrorMessage = "File is required")]
        [Display(Name = "File")]
        public IFormFile ExcelFile { get; set; }
    }
}
