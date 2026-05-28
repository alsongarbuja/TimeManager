using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimeManager.Backend.Models.Employee_Management
{
    public class JobProfile
    {
        [Key]
        public int Id { get; set; }

        public required int ProfileTemplateId { get; set; }

        public required int EmployeeId { get; set; }

        [ForeignKey("ProfileTemplateId")]
        public virtual ProfileTemplate ProfileTemplate { get; set; } = null!;

        [ForeignKey("EmployeeId")]
        public virtual Employee Employee { get; set; } = null!;
    }
}
