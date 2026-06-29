using System.ComponentModel.DataAnnotations;

namespace TimeManager.Backend.ViewModels
{
    public class EmployeeTypeViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [MinLength(3, ErrorMessage = "Name must be atleast 3 character long")]
        public string Name { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Description cannot exceed 100 characters")]
        [DataType(DataType.MultilineText)]
        public string? Description { get; set; }
    }
}
