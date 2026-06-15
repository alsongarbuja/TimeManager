using Microsoft.EntityFrameworkCore;
using TimeManager.Backend.Data;
using TimeManager.Backend.Models.Employee_Management;

namespace TimeManager.Backend.Services
{
    public interface IProfileTemplateService
    {
        Task<IEnumerable<ProfileTemplate>> GetProfileTemplatesAsync();
    }

    public class ProfileTemplateService : IProfileTemplateService
    {
        private readonly HrmsDbContext hrmsDbContext;

        public ProfileTemplateService(HrmsDbContext hrmsDbContext)
        {
            this.hrmsDbContext = hrmsDbContext;
        }
        
        public async Task<IEnumerable<ProfileTemplate>> GetProfileTemplatesAsync()
        {
            var profileTemplates = await this.hrmsDbContext.ProfileTemplate.Select(pt => new ProfileTemplate
            {
                Id = pt.Id,
                EmployeeTypeId = pt.EmployeeTypeId,
                UnitId = pt.UnitId,
                RoleId = pt.RoleId,
                PayFrequencyId = pt.PayFrequencyId,
                Unit = pt.Unit,
                Role = pt.Role,
                EmployeeType = pt.EmployeeType,
                PayFrequency = pt.PayFrequency,
                ShiftStartTime = pt.ShiftStartTime,
                EarlyClockInBufferMin = pt.EarlyClockInBufferMin,
            }).ToListAsync();
            return profileTemplates;
        }
    }
}
