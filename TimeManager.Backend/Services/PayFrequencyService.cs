using Microsoft.EntityFrameworkCore;
using TimeManager.Backend.Data;
using TimeManager.Backend.Models.Employee_Management;

namespace TimeManager.Backend.Services
{
    public interface IPayFrequencyService
    {
        Task<IEnumerable<PayFrequency>> GetPayFrequenciesAsync();
    }

    public class PayFrequencyService : IPayFrequencyService
    {
        private readonly HrmsDbContext hrmsDbContext;

        public PayFrequencyService(HrmsDbContext hrmsDbContext)
        {
            this.hrmsDbContext = hrmsDbContext;
        }

        public async Task<IEnumerable<PayFrequency>> GetPayFrequenciesAsync()
        {
            var payFrequencies = await this.hrmsDbContext.PayFrequency.ToListAsync();
            return payFrequencies;
        }
    }
}
