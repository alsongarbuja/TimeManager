using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeManager.Backend.Models.Requests;
using TimeManager.Backend.Models.Responses;
using TimeManager.Backend.Services;
using TimeManager.Backend.ViewModels;

namespace TimeManager.Backend.Controllers.PayPeriod
{
    [Authorize(Policy = "AdminPolicy")]
    public class PayPeriodController(IPayPeriodService payPeriodService) : Controller
    {
        public async Task<IActionResult> Index([FromQuery] PaginationFilter filter)
        {
            PagedResponse<PayPeriodViewModel> payperiods = await payPeriodService.GetPayPeriodsAsync(filter);
            return View(payperiods);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Generate() {
            await payPeriodService.AutoGeneratePayPeriod();

            return RedirectToAction(nameof(Index));
        }
    }
}
