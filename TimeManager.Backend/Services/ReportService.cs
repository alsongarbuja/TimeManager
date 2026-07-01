using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TimeManager.Backend.Controllers.PunchManagement.Utility;
using TimeManager.Backend.Data;
using TimeManager.Backend.Models.Employee_Management;
using TimeManager.Backend.Models.Organization_Management;
using TimeManager.Backend.Models.Punch_Management;
using TimeManager.Backend.ViewModels;

namespace TimeManager.Backend.Services
{
    public interface IReportService
    {
        //Task<IEnumerable<ReportViewModel>> GetReportsAsync();
        //Task<ReportDto> GetReportByIdAsync(int id);
        //Task CreateReportAsync(ReportDto reportDto);
        //Task<ReportDto?> UpdateReportAsync(int id, ReportDto reportDto);
        //Task<int?> DeleteReportByIdAsync(int id);
        Task<ReportGeneratedViewModel?> GenerateReportByJobProfileId(int id, int payPeriodId = 0);
        Task<IEnumerable<ReportGeneratedViewModel>> GenerateReportByUnitId(int id, int payPeriodId = 0);
    }

    public class ReportService(HrmsDbContext hrmsDbContext, PayPeriodUtility payPeriodUtility) : IReportService
    {
        public async Task<ReportGeneratedViewModel?> GenerateReportByJobProfileId(int id, int payPeriodId = 0)
        {
            JobProfile? jp = await hrmsDbContext.JobProfile.Include(jp => jp.Employee).AsSplitQuery().FirstOrDefaultAsync(j => j.Id == id);

            if (jp == null) return null;

            PayPeriod? pp = payPeriodId != 0 ? await payPeriodUtility.GetPayPeriodByIdAsync(payPeriodId) : await payPeriodUtility.GetPreviousPayPeriod();

            if (pp == null) return null;

            List<PunchEntry> punches = await hrmsDbContext.PunchEntry.Where(pe =>
                pe.JobProfileId == id && pe.ClockIn >= pp.StartDate && pe.ClockIn < pp.EndDate
                && pe.ClockOut > pp.StartDate && pe.ClockOut <= pp.EndDate && pe.ClockOut != null
            ).ToListAsync();

            ReportGeneratedViewModel report = new()
            {
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
                double totalHrs = Math.Round(punches[i].ClockOut!.Value.Subtract(punches[i].ClockIn).TotalHours, 2);

                report.TotalHours = Math.Round(report.TotalHours + totalHrs, 2);
                report.TotalWorkedHours = Math.Round(report.TotalWorkedHours + totalHrs, 2);

                bool isWeekTwo = (punches[i].ClockIn - pp.StartDate).Days >= 7;
                var targetWeek = isWeekTwo ? report.WeekTwo : report.WeekOne;

                if (targetWeek.TryGetValue(DayOfWeek, out ViewModels.DayReport? value))
                {
                    targetWeek[DayOfWeek] = new ViewModels.DayReport {
                        Hours = value.Hours + totalHrs,
                        Type = "REG",
                    };
                }
                else
                {
                    targetWeek.Add(DayOfWeek, new ViewModels.DayReport
                    {
                        Hours = totalHrs,
                        Type = "REG",
                    });
                }
            }

            string jsonString = JsonSerializer.Serialize(report, options: new(){ WriteIndented = true });

            return report;
        }

        public async Task<IEnumerable<ReportGeneratedViewModel>> GenerateReportByUnitId(int id, int payPeriodId = 0)
        {
            Unit? unit = await hrmsDbContext.Unit.FindAsync(id);
            if (unit == null) return [];

            PayPeriod? pp = payPeriodId != 0 ? await payPeriodUtility.GetPayPeriodByIdAsync(payPeriodId) : await payPeriodUtility.GetPreviousPayPeriod();

            if (pp == null) return [];

            List<ReportGeneratedViewModel> reports = [];

            List<JobProfile> jps = await hrmsDbContext.JobProfile.Where(jp => jp.ProfileTemplate.UnitId == id).ToListAsync();

            for (int i = 0; i < jps.Count; i++)
            {
                var r = await this.GenerateReportByJobProfileId(jps[i].Id, pp.Id);
                if (r == null) continue;
                reports.Add(r);
            }

            return reports;
        }
    }
}
