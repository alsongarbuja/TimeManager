using System.ComponentModel.DataAnnotations;

namespace TimeManager.Backend.Controllers.Department
{
    public class DepartmentDto
    {
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Description cannot be longer than 100")]
        public string? Description { get; set; }
    }
}
