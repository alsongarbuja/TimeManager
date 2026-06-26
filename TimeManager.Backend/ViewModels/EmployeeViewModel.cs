using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace TimeManager.Backend.ViewModels
{
    public class EmployeeViewModel
    {
        public int Id { get; set; }

        [Display(Name = "First Name")]
        [MinLength(3, ErrorMessage = "First Name must be atleast 3 character long")]
        [Required(ErrorMessage = "First Name is required")]
        public string FirstName { get; set; } = string.Empty;

        [Display(Name = "Last Name")]
        [MinLength(3, ErrorMessage = "Last Name must be atleast 3 character long")]
        [Required(ErrorMessage = "Last Name is required")]
        public string LastName { get; set; } = string.Empty;

        [Display(Name = "SO Id")]
        [Required(ErrorMessage = "The unique Id is required")]
        public string UniqueId { get; set; } = string.Empty;

        // For table and data listing
        public string Email { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;

        public string DepartmentName { get; set; } = string.Empty;
    }

    public class EmployeeData
    {
        public EmployeeViewModel EmployeeView { get; set; }

        [Display(Name = "User")]
        public int UserId { get; set; }

        [Display(Name = "Department")]
        public int DepartmentId { get; set; }

        public IEnumerable<SelectListItem> Departments { get; set; }
        public IEnumerable<SelectListItem> Users { get; set; }
    }
}
