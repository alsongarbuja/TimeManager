using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using TimeManager.Backend.Data;
using TimeManager.Backend.Extensions;
using TimeManager.Backend.Models.Employee_Management;
using TimeManager.Backend.ViewModels;

namespace TimeManager.Backend.Services
{
    public interface IProfileTemplateService
    {
        Task<IEnumerable<ProfileTemplateViewModel>> GetProfileTemplatesAsync(int? departmentId);
        Task<ProfileTemplate> GetProfileTemplateByIdAsync(int id);
        Task CreateProfileTemplateAsync(ProfileTemplateViewModel pvm);
        Task<ProfileTemplate?> UpdateProfileTemplateASync(int id, ProfileTemplateViewModel pvm);
        Task<int?> DeleteProfileTemplateAsync(int id);
        Task<IEnumerable<SelectListItem>> GetProfileTemplateOptionAsync(int selectedId = 0);
    }

    public class ProfileTemplateService(HrmsDbContext hrmsDbContext) : IProfileTemplateService
    {
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
            var profileTemplate = await hrmsDbContext.ProfileTemplate.FindOrThrowAsync(id);
            hrmsDbContext.ProfileTemplate.Remove(profileTemplate);
            await hrmsDbContext.SaveChangesAsync();
            return id;
        }

        public async Task<ProfileTemplate> GetProfileTemplateByIdAsync(int id)
        {
            return await hrmsDbContext.ProfileTemplate.FindOrThrowAsync(id);
        }

        public async Task<IEnumerable<SelectListItem>> GetProfileTemplateOptionAsync(int selectedId = 0)
        {
            var profileTemplates = await hrmsDbContext.ProfileTemplate.Select(pt => new SelectListItem
            {
                Text = $"{pt.Unit.Name} / {pt.Role.Name}",
                Value = pt.Id.ToString(),
                Selected = pt.Id == selectedId,
            }).ToListAsync();
            return profileTemplates;
        }

        public async Task<IEnumerable<ProfileTemplateViewModel>> GetProfileTemplatesAsync(int? departmentId)
        {
            IEnumerable<ProfileTemplateViewModel> profileTemplates = [];
            
            if (departmentId == null)
            {
                profileTemplates = await hrmsDbContext.ProfileTemplate.Select(pt => new ProfileTemplateViewModel
                {
                    Id = pt.Id,
                    Unit = pt.Unit.Name,
                    Role = pt.Role.Name ?? "Default",
                    EmployeeType = pt.EmployeeType.Name,
                    ShiftStartTime = pt.ShiftStartTime,
                    EarlyClockInBufferMin = pt.EarlyClockInBufferMin,
                }).ToListAsync();
            } else
            {
                profileTemplates = await hrmsDbContext.ProfileTemplate
                    .Where(pt => pt.Unit.DepartmentId == departmentId)
                    .Select(pt => new ProfileTemplateViewModel
                    {
                        Id = pt.Id,
                        Unit = pt.Unit.Name,
                        Role = pt.Role.Name ?? "Default",
                        EmployeeType = pt.EmployeeType.Name,
                        ShiftStartTime = pt.ShiftStartTime,
                        EarlyClockInBufferMin = pt.EarlyClockInBufferMin,
                    }).ToListAsync();
            }

            return profileTemplates;
        }

        public async Task<ProfileTemplate?> UpdateProfileTemplateASync(int id, ProfileTemplateViewModel pvm)
        {
            var profileTemplate = await hrmsDbContext.ProfileTemplate.FindAsync(id);
            if (profileTemplate == null) return null;
            hrmsDbContext.Entry(profileTemplate).CurrentValues.SetValues(pvm);
            await hrmsDbContext.SaveChangesAsync();
            return profileTemplate;
        }
    }

    public class ProfileTemplateDto
    {
        [Required(ErrorMessage = "Unit Id is required")]
        public int UnitId { get; set; }

        [Required(ErrorMessage = "Employee Type Id is required")]
        public int EmployeeTypeId { get; set; }

        [Required(ErrorMessage = "Pay Frequency Id is required")]
        public int PayFrequencyId { get; set; }

        [Required(ErrorMessage = "Role Id is required")]
        public int RoleId { get; set; }

        [Required(ErrorMessage = "Shift start time is required")]
        public TimeOnly ShiftStartTime { get; set; }

        public int EarlyClockInBufferMin { get; set; } = 5;
    }
}
