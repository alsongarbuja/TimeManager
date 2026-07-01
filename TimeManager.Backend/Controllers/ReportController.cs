using Microsoft.AspNetCore.Mvc;
using TimeManager.Backend.Extensions;
using TimeManager.Backend.Services;
using TimeManager.Backend.ViewModels;

namespace TimeManager.Backend.Controllers
{
    public class ReportController(IReportService reportService, IPayPeriodService payPeriodService, IJobProfileService jobProfileService, IUnitService unitService) : Controller
    {
        public async Task<IActionResult> Index()
        {
            int? departmentId = HttpContext.Session.GetDepartmentId();
            var payPeriods = await payPeriodService.GetPayPeriodOptionsAsync();
            var users = await jobProfileService.GetUserOptionsAsync(departmentId);
            var units = await unitService.GetUnitReportOptionsAsync(departmentId);
            
            return View(new ReportViewModel
            {
                PayPeriods = payPeriods,
                Users = users,
                Units = units,
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateReportByJPId(ReportViewModel reportViewModel)
        {
            var data = await reportService.GenerateReportByJobProfileId(reportViewModel.UserId ?? 0, reportViewModel.PayPeriodId ?? 0);
            return View("Result", data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateReportByUnitId(ReportViewModel rvm)
        {
            var d = await reportService.GenerateReportByUnitId(rvm.UnitId ?? 0, rvm.PayPeriodId ?? 0);
            return View("ResultByUnit", new ReportGeneratedUnitViewModel
            {
                Reports = [.. d],
            });
        }
    }
}
