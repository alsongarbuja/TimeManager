using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TimeManager.Backend.Controllers.PunchManagement.Dto;
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

    public class PayPeriodService : IPayPeriodService
    {
        private readonly HrmsDbContext _context;
        private readonly ILogger<PayPeriodService> _logger;
        private readonly PayPeriodUtility _payPeriodUtility;

        public PayPeriodService(HrmsDbContext context, ILogger<PayPeriodService> logger, PayPeriodUtility payPeriodUtility) {
            _context = context;
            _logger = logger;
            _payPeriodUtility = payPeriodUtility;
        }

        public async Task<IEnumerable<SelectListItem>> GetPayPeriodOptionsAsync()
        {
            var currentPp = await _payPeriodUtility.GetCurrentPayPeriod();
            var payPeriods = await _context.PayPeriod.Where(pp => pp.StartDate >= currentPp.StartDate).Select(pp => new SelectListItem
            {
                Value = pp.Id.ToString(),
                Text = $"{pp.StartDate.ToString("MMM d, yyyy")} - {pp.EndDate.ToString("MMM d, yyyy")}"
            }).ToListAsync();

            return payPeriods;
        }

        public async Task<IEnumerable<PayPeriodViewModel>> GetPayPeriodsAsync()
        {
            var currentPayPeriod = await _payPeriodUtility.GetCurrentPayPeriod();

            if (currentPayPeriod == null) return [];

            var payperiods = await _context.PayPeriod.Where(pp => currentPayPeriod.StartDate <= pp.StartDate).Select(p => new PayPeriodViewModel { 
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

            var latestPayPeriod = await _context.PayPeriod.OrderByDescending(p => p.EndDate)
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

                bool hasOverlap = await _context.PayPeriod.AnyAsync(p =>
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

                _context.PayPeriod.Add(newPeriod);

                createdCount++;

                DateTime nextStartDateComponent = currentEndLocalDate.Date.AddDays(1);
                currentStartLocalDt = new DateTimeOffset(nextStartDateComponent, localTz.GetUtcOffset(nextStartDateComponent));
            }

            if (createdCount > 0)
            {
                await _context.SaveChangesAsync();
            }
        }
    }
}
