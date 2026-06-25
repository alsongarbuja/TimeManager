using Microsoft.AspNetCore.Mvc;

namespace TimeManager.Backend.Controllers
{
    public class ErrorController : Controller
    {

        [Route("Error/{statusCode}")]
        public IActionResult Index(int statusCode)
        {
            return statusCode switch
            {
                404 => View("NotFound"),
                403 => View("~/Views/Auth/AccessDenied.cshtml"),
                _ => View("Error")
            };
        }
    }
}
