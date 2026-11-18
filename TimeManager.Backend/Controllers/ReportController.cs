using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using TimeManager.Backend.Common;
using TimeManager.Backend.Extensions;
using TimeManager.Backend.Services;
using TimeManager.Backend.ViewModels;
using PP = TimeManager.Backend.Models.Punch_Management.PayPeriod;

namespace TimeManager.Backend.Controllers
{
    public class ReportController(
        IReportService reportService, 
        IPayPeriodService payPeriodService, 
        IJobProfileService jobProfileService, 
        IUnitService unitService,
        ILogger<PP> logger
    ) : Controller
    {
        private static readonly string[] Days = ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];

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
            (_, var d) = await reportService.GenerateReportByUnitId(rvm.UnitId ?? 0, rvm.PayPeriodId ?? 0);
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
            (PP? pp, var data) = await reportService.GenerateReportByUnitId(rvm.UnitId ?? 0, rvm.PayPeriodId ?? 0);

            if (pp == null)
            {
                logger.LogWarning("Pay period not found");
                return NotFound();
            }

            var businessTz = TimeZoneInfo.FindSystemTimeZoneById(AppConstants.BUSINESS_TIMEZONE_ID);
            var localStart = TimeZoneInfo.ConvertTime(pp.StartDate, businessTz);
            var localEnd = TimeZoneInfo.ConvertTime(pp.EndDate, businessTz);

            List<DateTime> payPeriodDates = [];
            DateTime currentDate = localStart.Date;
            DateTime endDate = localEnd.Date;

            while (currentDate <= endDate)
            {
                payPeriodDates.Add(currentDate);
                currentDate = currentDate.AddDays(1);
            }

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add($"Report-{localStart:MMM dd} to {localEnd:MMM dd}");

            // Headers
            worksheet.Cell(1, 1).Value = "Name";
            int col = 2;

            foreach (var date in payPeriodDates)
            {
                worksheet.Cell(1, col).Value = date.ToString("ddd dd", System.Globalization.CultureInfo.InvariantCulture);
                col++;
            }
            worksheet.Cell(1, col).Value = "Total hrs";
            int totalHrsCol = col;

            // Styling
            var headerRange = worksheet.Range(1, 1, 1, totalHrsCol);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Font.FontColor = XLColor.White;
            headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#2F4F4F");
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // Data Rows
            int currentRow = 2;
            foreach (var d in data)
            {
                worksheet.Cell(currentRow, 1).Value = d.Name;
                int currentRowCol = 2;

                for (int i = 0; i < payPeriodDates.Count; i++)
                {
                    string dayName = payPeriodDates[i].DayOfWeek.ToString();
                    var weekDict = i < 7 ? d.WeekOne : d.WeekTwo;

                    if (weekDict.TryGetValue(dayName, out var entry))
                    {
                        worksheet.Cell(currentRow, currentRowCol).Value = $"{entry.Hours:F2} ({entry.Type})";
                    }
                    else
                    {
                        worksheet.Cell(currentRow, currentRowCol).Value = "-";
                    }
                    currentRowCol++;
                }

                worksheet.Cell(currentRow, totalHrsCol).Value = d.TotalHours;
                currentRow++;
            }

            worksheet.Column(1).Width = 20;
            for (int c = 2; c < totalHrsCol; c++) worksheet.Column(c).Width = 14;
            worksheet.Column(totalHrsCol).Width = 20;

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();

            string unitIndex = data.FirstOrDefault()?.UnitIndex ?? rvm.UnitId?.ToString() ?? "0";
            string fileName = $"Report_{unitIndex}_{localStart:MMM dd}_{localEnd:MMM dd}.xlsx";
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            TempData["success"] = "Successfully generated report excel";
            return File(content, contentType, fileName);

            //var tz = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
            //var localStart = TimeZoneInfo.ConvertTime(pp.StartDate, tz);
            //var localEnd = TimeZoneInfo.ConvertTime(pp.EndDate, tz);

            //List<DateTime> payPeriodDates = [];
            //DateTime currentDate = localStart.Date;
            //DateTime endDate = localEnd.Date;

            //while (currentDate < endDate)
            //{
            //    payPeriodDates.Add(currentDate);
            //    currentDate = currentDate.AddDays(1);
            //}

            //using var workbook = new XLWorkbook();
            //logger.LogInformation("Creating a new worksheet");
            //var worksheet = workbook.Worksheets.Add($"Report-{pp.StartDate:MMM dd} to {pp.EndDate:MMM dd}");

            //// Headers
            //int col = 2;
            //worksheet.Cell(1, 1).Value = "Name";

            //foreach (var date in payPeriodDates)
            //{
            //    worksheet.Cell(1, col).Value = date.ToString("ddd dd", System.Globalization.CultureInfo.InvariantCulture);
            //    col += 1;
            //}
            //worksheet.Cell(1, col).Value = "Total hrs";
            //int totalHrsCol = col;

            //// Styling the headers
            //var headerRange = worksheet.Range(1, 1, 1, col);
            //headerRange.Style.Font.Bold = true;
            //headerRange.Style.Font.FontColor = XLColor.White;
            //headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#2F4F4F");
            //headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            //// Datas
            //int currentRow = 2;
            //foreach (var d in data)
            //{
            //    logger.LogInformation("Adding data to each row");

            //    worksheet.Cell(currentRow, 1).Value = d.Name;
            //    int currentRowCol = 2;
            //    for (int i = 0; i < payPeriodDates.Count; i++)
            //    {
            //        string dayName = payPeriodDates[i].DayOfWeek.ToString();

            //        var weekDict = i < 7 ? d.WeekOne : d.WeekTwo;

            //        if (weekDict.TryGetValue(dayName, out var entry))
            //        {
            //            worksheet.Cell(currentRow, currentRowCol).Value = $"{entry.Hours:F2} ({entry.Type})";
            //        }
            //        else
            //        {
            //            worksheet.Cell(currentRow, currentRowCol).Value = "-";
            //        }
            //        currentRowCol += 1;
            //    }
            //    worksheet.Cell(currentRow, totalHrsCol).Value = d.TotalHours;
            //    currentRow += 1;
            //    // foreach (var day in Days)
            //    // {
            //    //     if (d.WeekOne.TryGetValue(day, out var entry))
            //    //     {
            //    //         worksheet.Cell(currentRow, currentRowCol).Value = $"{entry.Hours:F2} ({entry.Type})";
            //    //     }
            //    //     else
            //    //     {
            //    //         worksheet.Cell(currentRow, currentRowCol).Value = "-";
            //    //     }
            //    //     currentRowCol += 1;
            //    // }

            //    // foreach (var day in Days)
            //    // {
            //    //     if (d.WeekTwo.TryGetValue(day, out var entry))
            //    //     {
            //    //         worksheet.Cell(currentRow, currentRowCol).Value = $"{entry.Hours:F2} ({entry.Type})";
            //    //     }
            //    //     else
            //    //     {
            //    //         worksheet.Cell(currentRow, currentRowCol).Value = "-";
            //    //     }
            //    //     currentRowCol += 1;
            //    // }

            //    // worksheet.Cell(currentRow, currentRowCol).Value = d.TotalHours;
            //    // currentRow += 1;
            //}

            //worksheet.Column(1).Width = 20;
            //for (int c = 2; c < col; c++) worksheet.Column(c).Width = 14;
            //worksheet.Column(col).Width = 20;

            //using var stream = new MemoryStream();
            //logger.LogInformation("Saving and sending the excel file as stream");

            //workbook.SaveAs(stream);
            //var content = stream.ToArray();

            //var reportNameId = data.FirstOrDefault()?.UnitIndex ?? rvm.UnitId?.ToString();

            //string fileName = $"Report_{reportNameId}_{localStart:MMM dd}_{localEnd:MMM dd}.xlsx";
            //string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            //TempData["success"] = "Successfully generated report excel";
            //return File(content, contentType, fileName);
        }
    }
}
