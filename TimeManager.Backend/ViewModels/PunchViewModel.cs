using System.ComponentModel.DataAnnotations;

namespace TimeManager.Backend.ViewModels
{
    public class PunchViewOverall
    {
        public IEnumerable<Employees> employees { get; set; } = [];

        public IEnumerable<PunchViewModel> punches { get; set; } = [];
    }

    public class PunchViewModel
    {
        public int Id { get; set; }

        public int EmployeeId { get; set; }
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Clock in")]
        public DateTime ClockInTime { get; set; }
            
        [Display(Name = "Clock out")]
        public DateTime? ClockOutTime { get; set; }

    }

    public class Employees
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
    }
}
