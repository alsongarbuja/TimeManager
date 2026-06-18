using Microsoft.EntityFrameworkCore;
using TimeManager.Backend.Data;
using PP = TimeManager.Backend.Models.Punch_Management.PayPeriod;

namespace TimeManager.Backend.Controllers.PunchManagement.Utility
{
    public class PayPeriodUtility
    {
        private readonly HrmsDbContext _hrmsDbContext;

        public PayPeriodUtility(HrmsDbContext hrmsDbContext)
        {
            _hrmsDbContext = hrmsDbContext;
        }

        public async Task<PP?> GetCurrentPayPeriod() { 
            DateTimeOffset dateNow = DateTimeOffset.Now.UtcDateTime;

            PP? pp = await _hrmsDbContext.PayPeriod.Where(
                pp => dateNow >= pp.StartDate && dateNow <= pp.EndDate
                ).FirstOrDefaultAsync();

            return pp ?? null;
        }

        public async Task<PP?> GetPayPeriodByIdAsync(int id)
        {
            PP pp = await _hrmsDbContext.PayPeriod.FindAsync(id);
            return pp;
        }

        public async Task<Dictionary<string, string>> GetDateToDayMap()
        {
            var map = new Dictionary<string, string>();

            PP pp = await GetCurrentPayPeriod();

            return map;
        }
    }
}
