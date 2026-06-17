using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TimeManager.Backend.Controllers.PunchManagement.Utility;
using TimeManager.Backend.Data;
using TimeManager.Backend.Models.Punch_Management;

namespace TimeManager.Backend.Services
{
    public interface IPayPeriodService
    {
        Task<IEnumerable<PayPeriod>> GetPayPeriodsAsync();
        Task<IEnumerable<SelectListItem>> GetPayPeriodOptionsAsync();
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

        public async Task<IEnumerable<PayPeriod>> GetPayPeriodsAsync()
        {
            var currentPayPeriod = await _payPeriodUtility.GetCurrentPayPeriod();
            var payperiods = await _context.PayPeriod.Where(pp => currentPayPeriod.StartDate <= pp.StartDate).ToListAsync();
            return payperiods;
        }
    }
}
