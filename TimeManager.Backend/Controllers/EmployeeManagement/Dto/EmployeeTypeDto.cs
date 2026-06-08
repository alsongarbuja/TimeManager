using System.ComponentModel.DataAnnotations;

namespace TimeManager.Backend.Controllers.EmployeeManagement.Dto
{
    public class EmployeeTypeDto
    {
        [Required(ErrorMessage = "Name is required")]
        public string Name {  get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Description cannot exceed 100 characters")]
        public string Description { get; set; } = string.Empty;
    }
}
