using Microsoft.EntityFrameworkCore;
using TimeManager.Backend.Data;
using TimeManager.Backend.Models.Punch_Management;

namespace TimeManager.Backend.Controllers.PunchManagement.Utility
{
    public class PayPeriodUtility
    {
        private readonly HrmsDbContext _hrmsDbContext;

        public PayPeriodUtility(HrmsDbContext hrmsDbContext)
        {
            _hrmsDbContext = hrmsDbContext;
        }

        public async Task<PayPeriod> GetCurrentPayPeriod() { 
            DateTimeOffset dateNow = DateTimeOffset.Now.UtcDateTime;

            PayPeriod pp = await _hrmsDbContext.PayPeriod.Where(
                pp => dateNow >= pp.StartDate && dateNow <= pp.EndDate
                ).FirstAsync();

            return pp;
        }

        public async Task<PayPeriod?> GetPayPeriodByIdAsync(int id)
        {
            PayPeriod pp = await _hrmsDbContext.PayPeriod.FindAsync(id);
            return pp;
        }

        public async Task<Dictionary<string, string>> GetDateToDayMap()
        {
            var map = new Dictionary<string, string>();

            PayPeriod pp = await GetCurrentPayPeriod();

            return map;
        }
    }
}
