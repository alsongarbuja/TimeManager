using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeManager.Backend.Services;

namespace TimeManager.Backend.Controllers.PayPeriod
{
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class PayPeriodController : Controller
    {
        private readonly IPayPeriodService payPeriodService;

        public PayPeriodController(IPayPeriodService payPeriodService)
        {
            this.payPeriodService = payPeriodService;
        }

        public async Task<IActionResult> Index()
        {
            var payperiods = await payPeriodService.GetPayPeriodsAsync();
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
