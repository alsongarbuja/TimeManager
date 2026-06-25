using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TimeManager.Backend.Controllers.Scheduler
{
    [Authorize(Roles="SuperAdmin,Admin")]
    public class SchedulerController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
