using System.ComponentModel.DataAnnotations;

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

        [StringLength(50)]
        public required string Email { get; set; }

        public virtual ICollection<JobProfile> JobProfile { get; set; } = [];
    }
}
