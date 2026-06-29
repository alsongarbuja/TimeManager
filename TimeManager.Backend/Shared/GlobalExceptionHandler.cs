using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace TimeManager.Backend.Shared
{
    public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception ex,
            CancellationToken cancellationToken)
        {
            if (httpContext.Request.Path.StartsWithSegments("/error"))
            {
                return false;
            }

            logger.LogError(ex, "An unhandled exception occured: {Message}", ex.Message);

            var statusCode = ex switch
            {
                DbUpdateConcurrencyException => HttpStatusCode.Conflict,
                KeyNotFoundException => HttpStatusCode.NotFound,
                ArgumentException => HttpStatusCode.BadRequest,
                _ => HttpStatusCode.InternalServerError,
            };

            httpContext.Response.StatusCode = (int)statusCode;
            httpContext.Response.Redirect($"/Error/{(int)statusCode}");

            return true;
        }
    }
}
