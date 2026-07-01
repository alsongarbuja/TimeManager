using Microsoft.EntityFrameworkCore;
using TimeManager.Backend.Data;
using TimeManager.Backend.Extensions;
using PP = TimeManager.Backend.Models.Punch_Management.PayPeriod;

namespace TimeManager.Backend.Controllers.PunchManagement.Utility
{
    public class PayPeriodUtility(HrmsDbContext hrmsDbContext)
    {
        public async Task<PP?> GetCurrentPayPeriod() { 
            DateTimeOffset dateNow = DateTimeOffset.Now.UtcDateTime;

            PP? pp = await hrmsDbContext.PayPeriod.Where(
                pp => dateNow >= pp.StartDate && dateNow <= pp.EndDate
                ).FirstOrDefaultAsync();

            return pp ?? null;
        }

        public async Task<PP?> GetPreviousPayPeriod()
        {
            DateTimeOffset dateNow = DateTimeOffset.Now.UtcDateTime;

            PP? pp = await hrmsDbContext.PayPeriod.Where(
                    p => p.EndDate < dateNow
                ).OrderByDescending(p => p.EndDate).FirstOrDefaultAsync();

            return pp;
        }

        public async Task<PP?> GetPayPeriodByIdAsync(int id)
        {
            return await hrmsDbContext.PayPeriod.FindOrThrowAsync(id);
        }
    }
}
