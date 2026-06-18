using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimeManager.Backend.Controllers.PunchManagement.Dto;
using TimeManager.Backend.Data;
using PP = TimeManager.Backend.Models.Punch_Management.PayPeriod;

namespace TimeManager.Backend.Controllers.PunchManagement
{
    [ApiController]
    [Route("api/[controller]")]
    public class PayPeriodController: ControllerBase
    {
        private readonly HrmsDbContext _context;
        private readonly ILogger<PayPeriodController> _logger;

        public PayPeriodController(HrmsDbContext context, ILogger<PayPeriodController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PP>>> GetPayPeriods()
        {
            var payPeriods = await _context.PayPeriod.ToListAsync();
            return payPeriods;
        }

        [IgnoreAntiforgeryToken]
        [HttpPost("auto-generate")]
        public async Task AutoGeneratePayPeriod([FromBody] GenerateDto generateDto)
        {
            if (generateDto.numOfPeriods <= 0)
            {
                return;
            }

            TimeZoneInfo localTz = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");

            DateTimeOffset effectiveStartLocalDt;

            var latestPayPeriod = await _context.PayPeriod.OrderByDescending(p => p.EndDate)
                    .FirstOrDefaultAsync();

            if (latestPayPeriod != null)
            {
                var latestEndDateLocal = TimeZoneInfo.ConvertTime(latestPayPeriod.EndDate, localTz).Date;
                var nextStartDateComponent = latestEndDateLocal.AddDays(1);
                effectiveStartLocalDt = new DateTimeOffset(nextStartDateComponent, localTz.GetUtcOffset(nextStartDateComponent));
            } else
            {
                var seedDate = new DateTime(2025, 2, 9, 0, 0, 0);
                effectiveStartLocalDt = new DateTimeOffset(seedDate, localTz.GetUtcOffset(seedDate));
            }

            int createdCount = 0;
            DateTimeOffset currentStartLocalDt = effectiveStartLocalDt;
            
            for (int i = 0; i < generateDto.numOfPeriods; i++)
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
                    } else
                    {
                        currentEndLocalDate = new DateTimeOffset(endDateTime, localTz.GetUtcOffset(endDateTime));
                    }
                } catch (Exception)
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

                var newPeriod = new PP
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
