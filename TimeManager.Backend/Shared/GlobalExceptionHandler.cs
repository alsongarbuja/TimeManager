using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace TimeManager.Backend.Shared
{
    public class GlobalExceptionHandler: IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) {
            this._logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception ex,
            CancellationToken cancellationToken
            
            )
        {
            if (httpContext.Request.Path.StartsWithSegments("/error"))
            {
                return false;
            }

            _logger.LogError(ex, "An unhandled exception occured: {Message}", ex.Message);

            var statusCode = ex switch
            {
                DbUpdateConcurrencyException => HttpStatusCode.Conflict,
                KeyNotFoundException => HttpStatusCode.NotFound,
                ArgumentException => HttpStatusCode.BadRequest,
                _ => HttpStatusCode.InternalServerError,
            };


            var problemDetails = new ProblemDetails
            {
                Status = (int)statusCode,
                Title = statusCode == HttpStatusCode.InternalServerError ? "Server Error" : "Bad request",
                Detail = statusCode == HttpStatusCode.InternalServerError ? "An unexpected error occured to the server" : ex.Message,
            };

            httpContext.Response.StatusCode = problemDetails.Status.Value;

            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }
    }
}
