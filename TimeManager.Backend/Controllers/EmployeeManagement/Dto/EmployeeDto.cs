using System.ComponentModel.DataAnnotations;

namespace TimeManager.Backend.Controllers.EmployeeManagement.Dto
{
    public class EmployeeDto
    {
        [Required(ErrorMessage = "Unique Id is required")]
        [StringLength(20, ErrorMessage = "Unique Id cannot exceed 20 characters")]
        public string UniqueId { get; set; } = string.Empty;

        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [StringLength(50, ErrorMessage = "Email cannot exceed 50 characters")]
        public string Email { get; set; } = string.Empty;

    }
}
