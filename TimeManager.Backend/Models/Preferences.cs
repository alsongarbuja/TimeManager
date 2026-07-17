namespace TimeManager.Backend.Models
{
    public class PaginationSettings
    {
        public int Limit { get; set; } = 10;
        public string OrderBy { get; set; } = "Id";
        public bool IsOrderDescending { get; set; } = false;
    }

    public class Preferences
    {
        public PaginationSettings PunchesPref { get; set; } = new PaginationSettings { 
            OrderBy = "clock in",
            IsOrderDescending = true,
        };

        public PaginationSettings EmployeesPref { get; set; } = new PaginationSettings
        {
            OrderBy = "name",
        };

        public PaginationSettings JobProfilesPref { get; set; } = new PaginationSettings
        {
            OrderBy = "name",
        };

    }
}
