using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace TimeManager.Backend.ViewModels
{
    public class KioskViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; } = string.Empty;

        [Display(Name = "Allowed Ip address")]
        [Required (ErrorMessage = "Allowed Ip address cannot be empty")]
        public IPAddress AllowedIPAddress { get; set; }

        [Display(Name = "Department")]
        public int DepartmentId { get; set; }

        public string DepartmentName { get; set; } = string.Empty;

        public IEnumerable<SelectListItem> Departments { get; set; } = [];
    }
}
