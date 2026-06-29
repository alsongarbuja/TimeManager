using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using TimeManager.Backend.Data;
using TimeManager.Backend.Extensions;
using TimeManager.Backend.Models.Employee_Management;
using TimeManager.Backend.ViewModels;

namespace TimeManager.Backend.Services
{
    public interface IPayFrequencyService
    {
        Task<IEnumerable<PayFrequencyViewModel>> GetPayFrequenciesAsync();
        Task<PayFrequency> GetPayFrequencyByIdAsync(int id);
        Task<PayFrequency> GetPayFrequencyByNameAsync(string name);
        Task CreatePayFrequencyAsync(PayFrequencyDto payFrequencyDto);
        Task<PayFrequency?> UpdatePayFrequencyAsync(int id, PayFrequencyDto payFrequencyDto);
        Task<int?> DeletePayFrequencyByIdAsync(int id);
        Task<IEnumerable<SelectListItem>> GetPayFrequencyOptionsAsync();
    }

    public class PayFrequencyService(HrmsDbContext hrmsDbContext) : IPayFrequencyService
    {
        public async Task CreatePayFrequencyAsync(PayFrequencyDto payFrequencyDto)
        {
            hrmsDbContext.PayFrequency.Add(new PayFrequency { Name = payFrequencyDto.Name, Description = payFrequencyDto.Description });
            await hrmsDbContext.SaveChangesAsync();
        }

        public async Task<int?> DeletePayFrequencyByIdAsync(int id)
        {
            var pf = await hrmsDbContext.PayFrequency.FindOrThrowAsync(id);
            hrmsDbContext.PayFrequency.Remove(pf);
            await hrmsDbContext.SaveChangesAsync();
            return id;
        }

        public async Task<IEnumerable<PayFrequencyViewModel>> GetPayFrequenciesAsync()
        {
            var payFrequencies = await hrmsDbContext.PayFrequency.Select(pf => new PayFrequencyViewModel
            {
                Id = pf.Id,
                Name = pf.Name,
                Description = pf.Description,
            }).ToListAsync();
            return payFrequencies;
        }

        public async Task<PayFrequency> GetPayFrequencyByIdAsync(int id)
        {
            return await hrmsDbContext.PayFrequency.FindOrThrowAsync(id);
        }

        public async Task<PayFrequency> GetPayFrequencyByNameAsync(string name)
        {
            var pf = await hrmsDbContext.PayFrequency.Where(pf => pf.Name.Equals(name)).FirstOrDefaultAsync();
            return pf;
        }

        public async Task<IEnumerable<SelectListItem>> GetPayFrequencyOptionsAsync()
        {
            var payFrequencies = await hrmsDbContext.PayFrequency.Select(pf => new SelectListItem
            {
                Text = pf.Name,
                Value = pf.Id.ToString(),
            }).ToListAsync();
            return payFrequencies;
        }

        public async Task<PayFrequency?> UpdatePayFrequencyAsync(int id, PayFrequencyDto payFrequencyDto)
        {
            var pf = await hrmsDbContext.PayFrequency.FindAsync(id);
            if (pf == null) return null;

            hrmsDbContext.Entry(pf).CurrentValues.SetValues(payFrequencyDto);
            await hrmsDbContext.SaveChangesAsync();
            return pf;
        }
    }

    public class PayFrequencyDto
    {
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Description cannot exceed 100 characters")]
        public string? Description { get; set; } = string.Empty;
    }
}
