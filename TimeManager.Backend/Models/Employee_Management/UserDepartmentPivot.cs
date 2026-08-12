using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TimeManager.Backend.Models.AuthManagement;
using TimeManager.Backend.Models.Organization_Management;

namespace TimeManager.Backend.Models.Employee_Management
{
    public class UserDepartmentPivot
    {
        [Key]
        public int Id { get; set; }

        public required int UserId { get; set; }
        public required int DepartmentId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("DepartmentId")]
        public virtual Department Department { get; set; } = null!;
    }
}
