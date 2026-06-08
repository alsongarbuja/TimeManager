using System.ComponentModel.DataAnnotations;

namespace TimeManager.Backend.Controllers.Report.Dto
{
    public class ReportDto
    {
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Job profile Id is required")]
        public int JobProfileId { get; set; }

        [Required(ErrorMessage = "Total Hours required")]
        public double TotalHours { get; set; } = 0.0;

        [Required(ErrorMessage = "Total Worked Hours required")]
        public double TotalWorkedHours { get; set; } = 0.0;

        [Required(ErrorMessage = "Total Holiday Hours required")]
        public double TotalHolidayHours { get; set; } = 0.0;

        public Dictionary<string, DayReport> WeekOne { get; set; } = new();
        public Dictionary<string, DayReport> WeekTwo { get; set; } = new();
    }

    public class DayReport
    {
        public double Hours { get; set; } = 0.0;

        public string Type { get; set; } = string.Empty;
    }
}
