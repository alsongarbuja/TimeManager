using System.ComponentModel.DataAnnotations;

namespace TimeManager.Backend.Models.Punch_Management
{
    public class PayPeriod
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Start Date time offset is required")]
        public DateTimeOffset StartDate { get; set; }


        [Required(ErrorMessage = "End Date time offset is required")]
        public DateTimeOffset EndDate { get; set; }
    }
}
