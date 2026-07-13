namespace TimeManager.Backend.Models.Requests
{
    public class PaginationFilter
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public bool IsOrderDescending { get; set; } = false;
        public string OrderBy { get; set; } = string.Empty;
    }
}
