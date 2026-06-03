using System.ComponentModel.DataAnnotations;
using System.Net;
using TimeManager.Frontend.Components.Pages.Admin.Organization.Department;

namespace TimeManager.Frontend.Components.Pages.Admin.Device
{
    public class DeviceModel
    {
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Allowed Ip adrress is required")]
        public IPAddress AllowedIPAddress { get; set; }

        [StringLength(100, ErrorMessage = "Description cannot exceed 100 characters")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Department Id is required")]
        public int DepartmentId { get; set; }

        public DepartmentModel? Department { get; set; }
    }
}
