using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using TimeManager.Backend.Extensions;
using PP = TimeManager.Backend.Models.Punch_Management.PayPeriod;
using TimeManager.Backend.Services;
using TimeManager.Backend.ViewModels;
using TimeManager.Backend.Controllers.PunchManagement.Utility;

namespace TimeManager.Backend.Controllers
{
    public class ReportController(
        IReportService reportService, 
        IPayPeriodService payPeriodService, 
        IJobProfileService jobProfileService, 
        IUnitService unitService,
        PayPeriodUtility payPeriodUtility
    ) : Controller
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
                UnitId = rvm.UnitId ?? 0,
                PayPeriodId = rvm.PayPeriodId ?? 0
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExportToExcel(ReportViewModel rvm)
        {
            var data = await reportService.GenerateReportByUnitId(rvm.UnitId ?? 0, rvm.PayPeriodId ?? 0);
            PP? pp = rvm.PayPeriodId != null ? await payPeriodService.GetPayPeriodByIdAsync(rvm.PayPeriodId ?? 0) : await payPeriodUtility.GetCurrentPayPeriod();

            if (pp == null) return NotFound();

            List<string> dates = [];
            var days = new[] { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"  };

            DateTimeOffset currentDate = pp.StartDate;
            while (currentDate <= pp.EndDate)
            {
                dates.Add(currentDate.ToString("ddd dd"));
                currentDate = currentDate.AddDays(1);
            }

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add($"Report-{pp.StartDate:MMM dd} to {pp.EndDate:MMM dd}");

                // Headers
                int col = 2;
                worksheet.Cell(1, 1).Value = "Name";

                foreach(var date in dates)
                {
                    worksheet.Cell(1, col).Value = date;
                    col += 1;
                }
                worksheet.Cell(1, col).Value = "Total hrs";

                // Styling the headers
                var headerRange = worksheet.Range("A1:P1");
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Font.FontColor = XLColor.White;
                headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#2F4F4F");
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Datas
                int currentRow = 2;
                foreach(var d in data)
                {
                    worksheet.Cell(currentRow, 1).Value = d.Name;
                    int currentRowCol = 2;
                    foreach (var day in days)
                    {
                        if (d.WeekOne.TryGetValue(day, out var entry))
                        {
                            worksheet.Cell(currentRow, currentRowCol).Value = $"{entry.Hours:F2} ({entry.Type})";
                        }
                        else
                        {
                            worksheet.Cell(currentRow, currentRowCol).Value = "-";
                        }
                        currentRowCol += 1;
                    }

                    foreach (var day in days)
                    {
                        if (d.WeekTwo.TryGetValue(day, out var entry))
                        {
                            worksheet.Cell(currentRow, currentRowCol).Value = $"{entry.Hours:F2} ({entry.Type})";
                        }
                        else
                        {
                            worksheet.Cell(currentRow, currentRowCol).Value = "-";
                        }
                        currentRowCol += 1;
                    }

                    worksheet.Cell(currentRow, currentRowCol).Value = d.TotalHours;
                    currentRow += 1;
                }


                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();

                    string fileName = $"Report_{DateTime.Now.ToLocalTime():yyyyMMdd_HHmmss}.xlsx";
                    string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                    return File(content, contentType, fileName);
                }
            }
        }
    }
}
