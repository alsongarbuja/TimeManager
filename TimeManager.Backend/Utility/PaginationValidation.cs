using TimeManager.Backend.Models.Requests;

namespace TimeManager.Backend.Utility
{
    public class PaginationValidation
    {
        public static (int, int, string?, bool) ConvertToValidPaginationQueries(PaginationQuery queries) {
            int pageNumber = queries.PageNumber < 1 ? 1 : queries.PageNumber;
            int pageSize = queries.PageSize < 1 ? 10 : queries.PageSize;

            string? orderBy = queries.OrderBy ?? null;
            bool isOrderDescending = queries.IsOrderDescending;

            return (pageNumber, pageSize, orderBy, isOrderDescending);
        }
    }
}
