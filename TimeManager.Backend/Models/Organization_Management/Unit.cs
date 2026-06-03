using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TimeManager.Backend.Models.Employee_Management;

namespace TimeManager.Backend.Models.Organization_Management
{
    public class Unit
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(50, ErrorMessage = "Name cannot exceed 50 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Index is required")]
        public int Index { get; set; }

        [StringLength(100, ErrorMessage = "Description cannot exceed 100 characters")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Department Id is required")]
        public int DepartmentId { get; set; }

        [ForeignKey("DepartmentId")]
        public virtual Department Department { get; set; } = null!;

        public virtual ProfileTemplate? ProfileTemplate { get; set; }
    }
}
