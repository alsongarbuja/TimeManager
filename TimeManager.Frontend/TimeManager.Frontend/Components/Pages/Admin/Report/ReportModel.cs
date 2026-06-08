namespace TimeManager.Frontend.Components.Pages.Admin.Report
{
    public class ReportModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int JobProfileId { get; set; }
        public double TotalHours { get; set; }
        public double TotalWorkedHours { get; set; }
        public double TotalHolidayHours { get; set; }

        public Dictionary<string, DayReport> WeekOne { get; set; }
        public Dictionary<string, DayReport> WeekTwo { get; set; }
    }

    public class DayReport
    {
        public string Type { get; set; }
        public double Hours { get; set; }
    }
}
