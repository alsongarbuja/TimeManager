using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;
using TimeManager.Backend.Models.Organization_Management;

namespace TimeManager.Backend.Models.Device_Management
{
    public class Kiosk
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Name of the device is requried")]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;


        [StringLength(100, ErrorMessage = "Description cannot exceed 100 characters")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Allowed IP Address is required")]
        public required IPAddress AllowedIPAddress { get; set; }

        [Required(ErrorMessage = "Departmetn ID is required")]
        public int DepartmentId { get; set; }

        [ForeignKey("DepartmentId")]
        public virtual Department Department { get; set; } = null!;
    }
}
