using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace TimeManager.Backend.Utility
{
    public class ModelValidationLog
    {
        public static void LogModelStateValidationFailedLogs<T>(ILogger<T> logger, ModelStateDictionary modelState)
        {
            var errors = modelState.Where(x => x.Value?.Errors.Count > 0)
                           .Select(x => new
                           {
                               x.Key,
                               Errors = string.Join(", ", x.Value?.Errors.Select(e => e.ErrorMessage)!)
                           });

            foreach (var error in errors)
            {
                logger.LogWarning($"Field: {error.Key} | Error: {error.Errors}");
            }
        }
    }
}
