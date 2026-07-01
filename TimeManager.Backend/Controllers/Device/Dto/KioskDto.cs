using System.ComponentModel.DataAnnotations;
using System.Net;

namespace TimeManager.Backend.Controllers.Device.Dto
{
    public class KioskDto
    {
        [Required(ErrorMessage = "Department Id is required")]
        public int DepartmentId { get; set; }

        [Required(ErrorMessage = "Ip Address is required")]
        public required IPAddress AllowedIPAddress { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Description cannot exceed 100 characters")]
        public string Description { get; set; } = string.Empty;
    }

    public record KioskSessionResponse(
        string Token,
        string KioskName,
        int DepartmentId,
        string DepartmentName,
        DateTime ExpiresAt
    );
}
