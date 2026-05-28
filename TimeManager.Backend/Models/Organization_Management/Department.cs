using System.ComponentModel.DataAnnotations;

namespace TimeManager.Backend.Models.Organization_Management
{
    public class Department
    {
        [Key]
        public int Id { get; set; }

        [StringLength(100)]
        public required string Name { get; set; }

        [StringLength(100)]
        public string? Description { get; set; }

        public virtual ICollection<Unit> Unit { get; set; } = [];
    }
}
