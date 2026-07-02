namespace TimeManager.Backend.Models.Responses
{
    public class PagedResponse<T>(IEnumerable<T> data, int pageNumber, int pageSize, int totalRecords)
    {
        public IEnumerable<T> Data { get; set; } = data;
        public int PageNumber { get; set; } = pageNumber;
        public int PageSize { get; set; } = pageSize;
        public int TotalRecords { get; set; } = totalRecords;
        public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);
    }
}
