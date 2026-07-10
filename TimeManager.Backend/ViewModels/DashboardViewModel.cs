namespace TimeManager.Backend.ViewModels
{
    public class DashboardViewModel
    {
        public PunchStatusModel? PunchStatus { get; set; }

        public double TotalHours { get; set; } = 0.0;

        public Dictionary<string, DayReport> WeekOne { get; set; } = [];
        public Dictionary<string, DayReport> WeekTwo { get; set; } = [];
    }
}
