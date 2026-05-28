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

        [StringLength(50)]
        public required string Name { get; set; }

        [StringLength(100)]
        public string? Description { get; set; }

        public required IPAddress AllowedIPAddress { get; set; }

        public required int DepartmentId { get; set; }

        [ForeignKey("DepartmentId")]
        public virtual Department Department { get; set; } = null!;
    }
}
