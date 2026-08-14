using System.ComponentModel.DataAnnotations;

namespace TimeManager.Backend.ViewModels.PartialViews
{
    public class BulkEmployeeCreateViewModel
    {
        [Required(ErrorMessage = "File is required")]
        [Display(Name = "File")]
        public IFormFile ExcelFile { get; set; }
    }
}
