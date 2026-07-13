using Microsoft.EntityFrameworkCore;
using TimeManager.Backend.Controllers.PunchManagement.Utility;
using TimeManager.Backend.Data;
using TimeManager.Backend.Models.Employee_Management;
using TimeManager.Backend.Models.Punch_Management;
using TimeManager.Backend.ViewModels;

namespace TimeManager.Backend.Services
{
    public interface IReportService
    {
        Task<ReportGeneratedViewModel?> GenerateReportByJobProfileId(int id, int payPeriodId = 0);
        Task<IEnumerable<ReportGeneratedViewModel>> GenerateReportByUnitId(int id, int payPeriodId = 0);
    }

    public class ReportService(HrmsDbContext hrmsDbContext, PayPeriodUtility payPeriodUtility, ILogger<JobProfile> logger) : IReportService
    {
        public async Task<ReportGeneratedViewModel?> GenerateReportByJobProfileId(int id, int payPeriodId = 0)
        {
            JobProfile? jp = await hrmsDbContext.JobProfile.AsNoTracking().Include(jp => jp.Employee).Include(jp => jp.ProfileTemplate).ThenInclude(pt => pt.Unit).AsSplitQuery().FirstOrDefaultAsync(j => j.Id == id);

            if (jp == null)
            {
                logger.LogWarning($"Job profile with id: {id} not found");
                return null;
            }

            PayPeriod? pp = payPeriodId != 0 ? await payPeriodUtility.GetPayPeriodByIdAsync(payPeriodId) : await payPeriodUtility.GetCurrentPayPeriod();

            if (pp == null)
            {
                logger.LogWarning($"Pay period with id {payPeriodId} or previous pay period not found");
                return null;
            }
            List<PunchEntry> punches = await hrmsDbContext.PunchEntry.AsNoTracking().Where(pe =>
                pe.JobProfileId == id && pe.ClockIn >= pp.StartDate && pe.ClockIn < pp.EndDate
                && pe.ClockOut > pp.StartDate && pe.ClockOut <= pp.EndDate && pe.ClockOut != null
            ).ToListAsync();

            return BuildReport(jp, punches, pp);
        }

        public async Task<IEnumerable<ReportGeneratedViewModel>> GenerateReportByUnitId(int id, int payPeriodId = 0)
        {
            var unit = await hrmsDbContext.Unit.AsNoTracking().AnyAsync(u => u.Id == id);
            if (!unit)
            {
                logger.LogWarning($"Unit with id: {id} not found");
                return [];
            }

            PayPeriod? pp = payPeriodId != 0 ? await payPeriodUtility.GetPayPeriodByIdAsync(payPeriodId) : await payPeriodUtility.GetCurrentPayPeriod();

            if (pp == null)
            {
                logger.LogWarning($"Pay period with id: {payPeriodId} or previous pay period not found");
                return [];
            }

            List<ReportGeneratedViewModel> reports = [];

            List<JobProfile> jps = await hrmsDbContext.JobProfile
                .AsNoTracking()
                .Include(jp => jp.Employee)
                .Include(jp => jp.ProfileTemplate)
                .ThenInclude(pt => pt.Unit)
                .Where(jp => jp.ProfileTemplate.UnitId == id)
                .AsSplitQuery()
                .ToListAsync();

            if (!jps.Any())
            {
                return reports;
            }

            var jpIds = jps.Select(j => j.Id).ToList();
            var punches = await hrmsDbContext.PunchEntry
                .AsNoTracking()
                .Where(pe =>
                    jpIds.Contains(pe.JobProfileId) &&
                    pe.ClockIn >= pp.StartDate &&
                    pe.ClockIn < pp.EndDate &&
                    pe.ClockOut != null)
                .OrderBy(pe => pe.ClockIn)
                .ToListAsync();

            var punchesByProfile = punches
                .GroupBy(p => p.JobProfileId)
                .ToDictionary(
                    g => g.Key,
                    g => g.ToList()
                );

            foreach (var jp in jps)
            {
                punchesByProfile.TryGetValue(jp.Id, out var profile);

                reports.Add(BuildReport(jp, profile ?? [], pp));
            }
           
            return reports;
        }

        private static ReportGeneratedViewModel BuildReport(JobProfile jobProfile, IEnumerable<PunchEntry> punchEntries, PayPeriod payPeriod)
        {
            var report = new ReportGeneratedViewModel
            {
                Name = $"{jobProfile.Employee.FirstName} {jobProfile.Employee.LastName}",
                JobProfileId = jobProfile.Id,
                TotalHours = 0,
                TotalWorkedHours = 0,
                TotalHolidayHours = 0,
                WeekOne = [],
                WeekTwo = [],
                UnitName = jobProfile.ProfileTemplate.Unit.Name
            };

            foreach(var punch in punchEntries)
            {
                if (punch.ClockOut == null)
                    continue;

                var dayOfWeek = punch.ClockIn.DayOfWeek.ToString();
                var hours = Math.Round((punch.ClockOut.Value - punch.ClockIn).TotalHours, 2);

                report.TotalHours = Math.Round(report.TotalHours + hours, 2);
                report.TotalWorkedHours = Math.Round(report.TotalWorkedHours + hours, 2);

                bool isWeekTwo = (punch.ClockIn - payPeriod.StartDate).Days > 7;

                var targetWeek = isWeekTwo ? report.WeekTwo : report.WeekOne;

                if (targetWeek.TryGetValue(dayOfWeek, out var existingDayInWeek))
                {
                    existingDayInWeek.Hours += hours;
                } else
                {
                    targetWeek[dayOfWeek] = new DayReport
                    {
                        Hours = hours,
                        Type = "REG",
                    };
                }
            }

            return report;
        }
    }
}
