namespace TimeManager.Backend.Models.Responses
{
    public class PagedResponse<T>(IEnumerable<T> data, int pageNumber, int pageSize, int totalRecords, string? orderBy, bool isOrderDescending)
    {
        public IEnumerable<T> Data { get; set; } = data;
        public int PageNumber { get; set; } = pageNumber;
        public int PageSize { get; set; } = pageSize;
        public int TotalRecords { get; set; } = totalRecords;
        public string? OrderBy { get; set; } = orderBy;
        public bool IsOrderDescending { get; set; } = isOrderDescending;

        public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);
    }
}
