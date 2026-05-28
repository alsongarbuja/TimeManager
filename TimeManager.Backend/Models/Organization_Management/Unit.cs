using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TimeManager.Backend.Models.Employee_Management;

namespace TimeManager.Backend.Models.Organization_Management
{
    public class Unit
    {
        [Key]
        public int Id { get; set; }

        [StringLength(50)]
        public required string Name { get; set; }

        [StringLength(100)]
        public string? Description { get; set; }

        public required int DepartmentId { get; set; }

        [ForeignKey("DepartmentId")]
        public virtual Department Department { get; set; } = null!;

        public virtual ProfileTemplate? ProfileTemplate { get; set; }
    }
}
