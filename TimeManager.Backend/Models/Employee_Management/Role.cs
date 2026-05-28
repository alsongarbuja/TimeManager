using System.ComponentModel.DataAnnotations;

namespace TimeManager.Backend.Models.Employee_Management
{
    public class Role
    {
        [Key]
        public int Id { get; set; }

        [StringLength(20)]
        public required string Name { get; set; }

        [StringLength(200)]
        public string? Description { get; set; }

        public virtual ICollection<ProfileTemplate> ProfileTemplate { get; set; } = [];
    }
}
