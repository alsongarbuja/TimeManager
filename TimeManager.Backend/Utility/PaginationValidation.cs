using TimeManager.Backend.Models.Requests;

namespace TimeManager.Backend.Utility
{
    public class PaginationValidation
    {
        public static (int, int, string?, bool) ConvertToValidPaginationQueries(
            PaginationQuery queries, 
            PaginationQuery? defaultQueries = null
        ) {
            int pageNumber = queries.PageNumber < 1 ? 1 : queries.PageNumber;
            int pageSize = queries.PageSize < 1 ? 10 : queries.PageSize;
            string? orderBy = queries.OrderBy ?? null;
            bool isOrderDescending = queries.IsOrderDescending;

            if (defaultQueries != null)
            {
                Console.WriteLine($"pageNumber: {pageNumber}, pageSize: {pageSize}, orderBy: {orderBy}");
                if (pageSize == 10 && pageNumber == 1 && string.IsNullOrEmpty(orderBy))
                {
                    pageSize = defaultQueries.PageSize;
                    orderBy = defaultQueries.OrderBy;
                    isOrderDescending = defaultQueries.IsOrderDescending;
                }
            }

            return (pageNumber, pageSize, orderBy, isOrderDescending);
        }
    }
}
