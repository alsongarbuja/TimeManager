using System.ComponentModel.DataAnnotations;

namespace TimeManager.Backend.ViewModels
{
    public class BulkCreateModel
    {
        [Required(ErrorMessage = "File is required")]
        [Display(Name = "File")]
        public IFormFile ExcelFile { get; set; }

        public required string Controller { get; set; }
        public required string ExampleFileName { get; set; }
    }
}
