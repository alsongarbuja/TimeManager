using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimeManager.Backend.Models.Employee_Management
{
    public class JobHistory
    {
        [Key]
        public int Id { get; set; }
        public required int JobProfileId { get; set; }
        public required int ProfileTemplateId { get; set; }
        public DateTime JoinDate { get; set; }
        public DateTime? EndDate { get; set; }

        [ForeignKey("JobProfileId")]
        public virtual JobProfile JobProfile { get; set; } = null!;

        [ForeignKey("ProfileTemplateId")]
        public virtual ProfileTemplate ProfileTemplate { get; set; } = null!;
    }
}
