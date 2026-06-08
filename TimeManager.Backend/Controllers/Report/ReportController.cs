using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TimeManager.Backend.Controllers.PunchManagement.Utility;
using TimeManager.Backend.Controllers.Report.Dto;
using TimeManager.Backend.Data;
using TimeManager.Backend.Models.Employee_Management;
using TimeManager.Backend.Models.Punch_Management;

namespace TimeManager.Backend.Controllers.Report
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportController: ControllerBase
    {
        private readonly HrmsDbContext _context;
        private readonly ILogger<ReportController> _logger;
        private readonly PayPeriodUtility _payPeriodUtility;

        public ReportController(HrmsDbContext context, ILogger<ReportController> logger, PayPeriodUtility payPeriodUtility)
        {
            _context = context;
            _logger = logger;
            _payPeriodUtility = payPeriodUtility;
        }

        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<ReportDto>>> GetReport()
        //{
        //    ReportDto report = new ReportDto();

        //    //return [report];
        //}

        [HttpGet("{id}")]
        public async Task<ActionResult<ReportDto>> GetReportByJobProfileId(int id) {
            JobProfile jp = await _context.JobProfile.Include(jp => jp.Employee).FirstOrDefaultAsync(j => j.Id == id);

            if (jp == null)
            {
                return BadRequest("No job profile found with the given Id");
            }

            PayPeriod pp = await _payPeriodUtility.GetCurrentPayPeriod();

            List<PunchEntry> punches = await _context.PunchEntry.Where(pe =>
                pe.JobProfileId == id && pe.ClockIn >= pp.StartDate && pe.ClockIn < pp.EndDate
                && pe.ClockOut > pp.StartDate && pe.ClockOut <= pp.EndDate && pe.ClockOut != null
            ).ToListAsync();

            ReportDto report = new ReportDto {
                Name = $"{jp.Employee.FirstName} {jp.Employee.LastName}",
                JobProfileId = id,
                TotalHours = 0.0,
                TotalHolidayHours = 0.0,
                TotalWorkedHours = 0.0,
                WeekOne = [],
                WeekTwo = [],
            };

            for (int i = 0; i < punches.Count; i++)
            {
                string DayOfWeek = punches[i].ClockIn.DayOfWeek.ToString();
                double totalHrs = Math.Round(punches[i].ClockOut.Value.Subtract(punches[i].ClockIn).TotalHours, 2);

                report.TotalHours = Math.Round(report.TotalHours+totalHrs, 2);
                report.TotalWorkedHours = Math.Round(report.TotalWorkedHours+totalHrs, 2);

                bool isWeekTwo = (punches[i].ClockIn - pp.StartDate).Days >= 7;
                var targetWeek = isWeekTwo ? report.WeekTwo : report.WeekOne;

                if (targetWeek.TryGetValue(DayOfWeek, out DayReport? value))
                {
                    targetWeek[DayOfWeek] = new DayReport
                    {
                        Hours = value.Hours + totalHrs,
                        Type = "REG",
                    };
                } else
                {
                    targetWeek.Add(DayOfWeek, new DayReport
                    {
                        Hours = totalHrs,
                        Type = "REG",
                    });
                }
            }

            string jsonString = JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true });

            Console.WriteLine(jsonString);

            return report;
        }
    }
}
