using Microsoft.EntityFrameworkCore;
using TimeManager.Backend.Controllers.EmployeeManagement.Dto;
using TimeManager.Backend.Data;
using TimeManager.Backend.Models.Employee_Management;
using TimeManager.Backend.ViewModels;

namespace TimeManager.Backend.Services
{
    public interface IPayFrequencyService
    {
        Task<IEnumerable<PayFrequencyViewModel>> GetPayFrequenciesAsync();
        Task<PayFrequency> GetPayFrequencyByIdAsync(int id);
        Task CreatePayFrequencyAsync(PayFrequencyDto payFrequencyDto);
        Task<PayFrequency?> UpdatePayFrequencyAsync(int id, PayFrequencyDto payFrequencyDto);
        Task<int?> DeletePayFrequencyByIdAsync(int id);
    }

    public class PayFrequencyService : IPayFrequencyService
    {
        private readonly HrmsDbContext hrmsDbContext;

        public PayFrequencyService(HrmsDbContext hrmsDbContext)
        {
            this.hrmsDbContext = hrmsDbContext;
        }

        public async Task CreatePayFrequencyAsync(PayFrequencyDto payFrequencyDto)
        {
            this.hrmsDbContext.PayFrequency.Add(new PayFrequency { Name = payFrequencyDto.Name, Description = payFrequencyDto.Description });
            await this.hrmsDbContext.SaveChangesAsync();
        }

        public async Task<int?> DeletePayFrequencyByIdAsync(int id)
        {
            var pf = await this.hrmsDbContext.PayFrequency.FindAsync(id);
            if (pf == null) return null;

            this.hrmsDbContext.PayFrequency.Remove(pf);
            await this.hrmsDbContext.SaveChangesAsync();
            return id;
        }

        public async Task<IEnumerable<PayFrequencyViewModel>> GetPayFrequenciesAsync()
        {
            var payFrequencies = await this.hrmsDbContext.PayFrequency.Select(pf => new PayFrequencyViewModel
            {
                Id = pf.Id,
                Name = pf.Name,
                Description = pf.Description,
            }).ToListAsync();
            return payFrequencies;
        }

        public async Task<PayFrequency> GetPayFrequencyByIdAsync(int id)
        {
            var pf = await this.hrmsDbContext.PayFrequency.FindAsync(id);
            return pf;
        }

        public async Task<PayFrequency?> UpdatePayFrequencyAsync(int id, PayFrequencyDto payFrequencyDto)
        {
            var pf = await this.hrmsDbContext.PayFrequency.FindAsync(id);
            if (pf == null) return null;

            this.hrmsDbContext.Entry(pf).CurrentValues.SetValues(payFrequencyDto);
            await this.hrmsDbContext.SaveChangesAsync();
            return pf;
        }
    }
}
