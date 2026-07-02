using TimeManager.Backend.Models.Requests;

namespace TimeManager.Backend.Utility
{
    public class PaginationValidation
    {
        public static (int, int) ValidateFilterValues(PaginationFilter filter) {
            int pageNumber = filter.PageNumber < 1 ? 1 : filter.PageNumber;
            int pageSize = filter.PageSize < 1 ? 10 : filter.PageSize;

            return (pageNumber, pageSize);
        }
    }
}
