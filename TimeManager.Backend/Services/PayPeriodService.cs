using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TimeManager.Backend.Controllers.PunchManagement.Utility;
using TimeManager.Backend.Data;
using TimeManager.Backend.Models.Punch_Management;
using TimeManager.Backend.ViewModels;

namespace TimeManager.Backend.Services
{
    public interface IPayPeriodService
    {
        Task<IEnumerable<PayPeriodViewModel>> GetPayPeriodsAsync();
        Task<IEnumerable<SelectListItem>> GetPayPeriodOptionsAsync();
        Task AutoGeneratePayPeriod();
    }

    public class PayPeriodService(HrmsDbContext context, ILogger<PayPeriodService> logger, PayPeriodUtility payPeriodUtility) : IPayPeriodService
    {
        public async Task<IEnumerable<SelectListItem>> GetPayPeriodOptionsAsync()
        {
            var currentPp = await payPeriodUtility.GetCurrentPayPeriod();
            var payPeriods = await context.PayPeriod.Where(pp => pp.StartDate <= currentPp.StartDate).OrderByDescending(pp => pp.StartDate).Select(pp => new SelectListItem
            {
                Value = pp.Id.ToString(),
                Text = $"{pp.StartDate:MMM d, yyyy} - {pp.EndDate:MMM d, yyyy}",
                Selected = pp.Id == currentPp.Id
            }).ToListAsync();

            return payPeriods;
        }

        public async Task<IEnumerable<PayPeriodViewModel>> GetPayPeriodsAsync()
        {
            var currentPayPeriod = await payPeriodUtility.GetCurrentPayPeriod();

            if (currentPayPeriod == null) return [];

            var payperiods = await context.PayPeriod.Where(pp => currentPayPeriod.StartDate <= pp.StartDate).Select(p => new PayPeriodViewModel { 
                StartDate = p.StartDate.ToString("MMM d yyyy"),
                EndDate = p.EndDate.ToString("MMM d yyyy")
            }).ToListAsync();
            return payperiods;
        }

        public async Task AutoGeneratePayPeriod()
        {
            int numOfPeriods = 30;
            TimeZoneInfo localTz = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");

            DateTimeOffset effectiveStartLocalDt;

            var latestPayPeriod = await context.PayPeriod.OrderByDescending(p => p.EndDate)
                    .FirstOrDefaultAsync();

            if (latestPayPeriod != null)
            {
                var latestEndDateLocal = TimeZoneInfo.ConvertTime(latestPayPeriod.EndDate, localTz).Date;
                var nextStartDateComponent = latestEndDateLocal.AddDays(1);
                effectiveStartLocalDt = new DateTimeOffset(nextStartDateComponent, localTz.GetUtcOffset(nextStartDateComponent));
            }
            else
            {
                var seedDate = new DateTime(2025, 2, 9, 0, 0, 0);
                effectiveStartLocalDt = new DateTimeOffset(seedDate, localTz.GetUtcOffset(seedDate));
            }

            int createdCount = 0;
            DateTimeOffset currentStartLocalDt = effectiveStartLocalDt;

            for (int i = 0; i < numOfPeriods; i++)
            {
                DateTime currentEndDateComponent = currentStartLocalDt.Date.AddDays(13);
                DateTimeOffset currentEndLocalDate;

                try
                {
                    var endDateTime = new DateTime(currentEndDateComponent.Year, currentEndDateComponent.Month, currentEndDateComponent.Day, 23, 59, 59);

                    if (localTz.IsInvalidTime(endDateTime))
                    {
                        var nextDayMidnight = new DateTime(currentEndDateComponent.Year, currentEndDateComponent.Month, currentEndDateComponent.Day).AddDays(1);
                        var nextDayMidnightOffset = new DateTimeOffset(nextDayMidnight, localTz.GetUtcOffset(nextDayMidnight));
                        currentEndLocalDate = nextDayMidnightOffset.AddSeconds(-1);
                    }
                    else
                    {
                        currentEndLocalDate = new DateTimeOffset(endDateTime, localTz.GetUtcOffset(endDateTime));
                    }
                }
                catch (Exception)
                {
                    var nextDayMidnight = new DateTime(currentEndDateComponent.Year, currentEndDateComponent.Month, currentEndDateComponent.Day).AddDays(1);
                    var nextDayMidnightOffset = new DateTimeOffset(nextDayMidnight, localTz.GetUtcOffset(nextDayMidnight));
                    currentEndLocalDate = nextDayMidnightOffset.AddSeconds(-1);
                }

                DateTimeOffset startDateUtc = currentStartLocalDt.ToUniversalTime();
                DateTimeOffset endDateUtc = currentEndLocalDate.ToUniversalTime();

                bool hasOverlap = await context.PayPeriod.AnyAsync(p =>
                    p.StartDate <= endDateUtc && p.EndDate >= startDateUtc &&
                    !(p.StartDate == startDateUtc && p.EndDate == endDateUtc));

                if (hasOverlap)
                {
                    Console.WriteLine($"Overlap detected for period {startDateUtc} to {endDateUtc}. Stopping generation");
                    break;
                }

                var newPeriod = new PayPeriod
                {
                    StartDate = startDateUtc,
                    EndDate = endDateUtc,
                };

                context.PayPeriod.Add(newPeriod);

                createdCount++;

                DateTime nextStartDateComponent = currentEndLocalDate.Date.AddDays(1);
                currentStartLocalDt = new DateTimeOffset(nextStartDateComponent, localTz.GetUtcOffset(nextStartDateComponent));
            }

            if (createdCount > 0)
            {
                await context.SaveChangesAsync();
            }
        }
    }
}
