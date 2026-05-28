using System.ComponentModel.DataAnnotations;

namespace TimeManager.Backend.Models.Punch_Management
{
    public class PayPeriod
    {
        [Key]
        public int Id { get; set; }

        public required DateTime StartDate { get; set; }

        public required DateTime EndDate { get; set; }
    }
}
