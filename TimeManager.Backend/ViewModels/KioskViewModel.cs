using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace TimeManager.Backend.ViewModels
{
    public class KioskViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Name")]
        [Required(ErrorMessage = "Name is required")]
        [MinLength(3, ErrorMessage = "Name must be atleast 3 character long")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Description")]
        [StringLength(100, ErrorMessage = "Description cannot exceed 100 characters")]
        [DataType(DataType.MultilineText)]
        public string? Description { get; set; } = string.Empty;

        [Display(Name = "Allowed Ip address")]
        [Required (ErrorMessage = "Allowed Ip address cannot be empty")]
        public required IPAddress AllowedIPAddress { get; set; }

        [Display(Name = "Department")]
        public int DepartmentId { get; set; }

        public string DepartmentName { get; set; } = string.Empty;

        public IEnumerable<SelectListItem> Departments { get; set; } = [];
    }
}
