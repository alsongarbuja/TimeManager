using Microsoft.EntityFrameworkCore;
using TimeManager.Backend.Controllers.PunchManagement.Utility;
using TimeManager.Backend.Data;
using TimeManager.Backend.Models.Punch_Management;

namespace TimeManager.Backend.Services
{
    public interface IPayPeriodService
    {
        Task<IEnumerable<PayPeriod>> GetPayPeriodsAsync();
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

        public async Task<IEnumerable<PayPeriod>> GetPayPeriodsAsync()
        {
            var currentPayPeriod = await _payPeriodUtility.GetCurrentPayPeriod();
            var payperiods = await _context.PayPeriod.Where(pp => currentPayPeriod.StartDate <= pp.StartDate).ToListAsync();
            return payperiods;
        }
    }
}
