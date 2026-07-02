using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace TimeManager.Backend.ViewModels
{
    public class ReportViewModel
    {
        public IEnumerable<SelectListItem> PayPeriods { get; set; } = [];
        public IEnumerable<SelectListItem> Units { get; set; } = [];
        public IEnumerable<SelectListItem> Users { get; set; } = [];

        [Display(Name = "Pay Period")]
        public int? PayPeriodId { get; set; }

        [Display(Name = "Unit")]
        public int? UnitId { get; set; }

        [Display(Name = "Employee")]
        public int? UserId { get; set; }
    }

    public class ReportGeneratedViewModel
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Job profile Id is required")]
        public int JobProfileId { get; set; }

        public string UnitName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Total Hours required")]
        public double TotalHours { get; set; } = 0.0;

        [Required(ErrorMessage = "Total Worked Hours required")]
        public double TotalWorkedHours { get; set; } = 0.0;

        [Required(ErrorMessage = "Total Holiday Hours required")]
        public double TotalHolidayHours { get; set; } = 0.0;

        public Dictionary<string, DayReport> WeekOne { get; set; } = [];
        public Dictionary<string, DayReport> WeekTwo { get; set; } = [];
    }

    public class ReportGeneratedUnitViewModel
    {
        [Required]
        public List<ReportGeneratedViewModel> Reports { get; set; } = [];

        public int UnitId { get; set; }

        public int PayPeriodId { get; set; }
    }

    public class DayReport
    {
        public double Hours { get; set; } = 0.0;

        public string Type { get; set; } = string.Empty;
    }

    public class PayPeriodReportOption
    {
        public int Id { get; set; }

        public string PayPeriodName { get; set; } = string.Empty;
    }

    public class UnitReportOption
    {
        public int Id { get; set; }

        public string UnitName { get; set; } = string.Empty;
    }

    public class EmployeeReportOption
    {
        public int Id { get; set; }

        public string EmployeeName { get; set; } = string.Empty;
    }
}
