using Microsoft.AspNetCore.Mvc;

namespace TimeManager.Backend.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
