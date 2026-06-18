using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TimeManager.Backend.Data;
using TimeManager.Backend.Models.Employee_Management;
using TimeManager.Backend.ViewModels;

namespace TimeManager.Backend.Services
{
    public interface IProfileTemplateService
    {
        Task<IEnumerable<ProfileTemplateViewModel>> GetProfileTemplatesAsync();
        Task<ProfileTemplate> GetProfileTemplateByIdAsync(int id);
        Task CreateProfileTemplateAsync(ProfileTemplateViewModel pvm);
        Task<ProfileTemplate?> UpdateProfileTemplateASync(int id, ProfileTemplateViewModel pvm);
        Task<int?> DeleteProfileTemplateAsync(int id);
        Task<IEnumerable<SelectListItem>> GetProfileTemplateOptionAsync();
    }

    public class ProfileTemplateService : IProfileTemplateService
    {
        private readonly HrmsDbContext hrmsDbContext;

        public ProfileTemplateService(HrmsDbContext hrmsDbContext)
        {
            this.hrmsDbContext = hrmsDbContext;
        }

        public async Task CreateProfileTemplateAsync(ProfileTemplateViewModel pvm)
        {
            hrmsDbContext.ProfileTemplate.Add(new ProfileTemplate {
                EmployeeTypeId = pvm.EmployeeTypeId,
                RoleId = pvm.RoleId,
                UnitId = pvm.UnitId,
                PayFrequencyId = pvm.PayFrequencyId,
                EarlyClockInBufferMin = pvm.EarlyClockInBufferMin,
                ShiftStartTime = pvm.ShiftStartTime,
            });
            await hrmsDbContext.SaveChangesAsync();
        }

        public async Task<int?> DeleteProfileTemplateAsync(int id)
        {
            var profileTemplate = await this.hrmsDbContext.ProfileTemplate.FindAsync(id);
            if (profileTemplate == null) return null;

            hrmsDbContext.ProfileTemplate.Remove(profileTemplate);
            await hrmsDbContext.SaveChangesAsync();
            return id;
        }

        public async Task<ProfileTemplate> GetProfileTemplateByIdAsync(int id)
        {
            var profileTemplate = await this.hrmsDbContext.ProfileTemplate.FindAsync(id);
            return profileTemplate;
        }

        public async Task<IEnumerable<SelectListItem>> GetProfileTemplateOptionAsync()
        {
            var profileTemplates = await this.hrmsDbContext.ProfileTemplate.Select(pt => new SelectListItem
            {
                Text = $"{pt.Unit.Name} / {pt.Role.Name}",
                Value = pt.Id.ToString(),
            }).ToListAsync();
            return profileTemplates;
        }

        public async Task<IEnumerable<ProfileTemplateViewModel>> GetProfileTemplatesAsync()
        {
            var profileTemplates = await this.hrmsDbContext.ProfileTemplate.Select(pt => new ProfileTemplateViewModel
            {
                Id = pt.Id,
                Unit = pt.Unit.Name,
                Role = pt.Role.Name ?? "Default",
                EmployeeType = pt.EmployeeType.Name,
                ShiftStartTime = pt.ShiftStartTime,
                EarlyClockInBufferMin = pt.EarlyClockInBufferMin,
            }).ToListAsync();
            return profileTemplates;
        }

        public async Task<ProfileTemplate?> UpdateProfileTemplateASync(int id, ProfileTemplateViewModel pvm)
        {
            var profileTemplate = await this.hrmsDbContext.ProfileTemplate.FindAsync(id);
            if (profileTemplate == null) return null;
            this.hrmsDbContext.Entry(profileTemplate).CurrentValues.SetValues(pvm);
            await this.hrmsDbContext.SaveChangesAsync();
            return profileTemplate;
        }
    }
}
