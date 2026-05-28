using System.ComponentModel.DataAnnotations;

namespace TimeManager.Backend.Models.Employee_Management
{
    public class PayFrequency
    {
        [Key]
        public int Id { get; set; }

        [StringLength(50)]
        public required string Name { get; set; }

        [StringLength(100)]
        public string? Description { get; set; }

        public virtual ProfileTemplate? ProfileTemplate { get; set; }
    }
}
