using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TimeManager.Backend.Models.AuthManagement;
using TimeManager.Backend.Models.Organization_Management;

namespace TimeManager.Backend.Models.Employee_Management
{
    public class Employee
    {
        [Key]
        public int Id { get; set; }

        [StringLength(20)]
        public required string UniqueId { get; set; }

        [StringLength(50)]
        public required string FirstName { get; set; }

        [StringLength(50)]
        public required string LastName { get; set; }

        [Required(ErrorMessage = "User Id is required")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Department Id is required")]
        public int DepartmentId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("DepartmentId")]
        public virtual Department Department { get; set; } = null!;

        //public ICollection<Department> EmployeeDepartments { get; set; } = [];
        public virtual ICollection<JobProfile> JobProfile { get; set; } = [];
    }
}
