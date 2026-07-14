namespace TimeManager.Backend.ViewModels.PartialViews
{
    public class ReportTablePartialViewModel
    {
        public double TotalHours { get; set; } = 0.0;
        public ReportTableWeekRow WeekOne { get; set; }
        public ReportTableWeekRow WeekTwo { get; set; }
    }

    public class ReportTableWeekRow
    {
        public string Label { get; set; } = string.Empty;
        public Dictionary<string, DayReport> Week { get; set; }
    }

    public class ReportStatModel 
    {
        public double TotalHours { get; set; } = 0.0;
        public double TotalWorkedHours { get; set; } = 0.0;
        public double TotalHolidayHours { get; set; } = 0.0;
    }
}
