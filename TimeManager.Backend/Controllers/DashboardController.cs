using Microsoft.AspNetCore.Mvc;
using TimeManager.Backend.Extensions;
using TimeManager.Backend.Services;

namespace TimeManager.Backend.Controllers
{
    public class DashboardController(IDashboardService dashboardService) : Controller
    {
        public async Task<IActionResult> Index()
        {
            int? jobProfileId = HttpContext.Session.GetCurrentUserJobProfileId();
            var data = await dashboardService.GetCurrentUserDashboardData(jobProfileId ?? 0);
            return View(data);
        }
    }
}
