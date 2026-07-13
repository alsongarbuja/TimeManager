using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using TimeManager.Backend.Data;
using TimeManager.Backend.Extensions;
using TimeManager.Backend.Models.Employee_Management;
using TimeManager.Backend.Models.Requests;
using TimeManager.Backend.Models.Responses;
using TimeManager.Backend.Utility;
using TimeManager.Backend.ViewModels;

namespace TimeManager.Backend.Services
{
    public interface IProfileTemplateService
    {
        Task<PagedResponse<ProfileTemplateViewModel>> GetProfileTemplatesAsync(int? departmentId, PaginationFilter filter);
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
                Text = $"{pt.Unit.Name} ({pt.Unit.Index}) / {pt.Role.Name}",
                Value = pt.Id.ToString(),
                Selected = pt.Id == selectedId,
            }).ToListAsync();
            return profileTemplates;
        }

        public async Task<PagedResponse<ProfileTemplateViewModel>> GetProfileTemplatesAsync(int? departmentId, PaginationFilter filter)
        {
            (int pageNumber, int pageSize, string? orderBy, bool isOrderDescending) = PaginationValidation.ValidateFilterValues(filter);

            int totalRecords = 0;

            IEnumerable<ProfileTemplateViewModel> profileTemplates = [];
            Expression<Func<ProfileTemplate, object>>? orderExpression = orderBy?.ToLower() switch
            {
                "unit" => pt => pt.Unit.Name,
                "role" => pt => pt.Role.Name,
                _ => null,
            };
            
            if (departmentId == null)
            {
                (profileTemplates, totalRecords) = await hrmsDbContext.ProfileTemplate.FindWithPaginationAsync(
                 pt => new ProfileTemplateViewModel
                 {
                     Id = pt.Id,
                     Unit = $"{pt.Unit.Name} - {pt.Unit.Index}",
                     Role = pt.Role.Name ?? "Default",
                     EmployeeType = pt.EmployeeType.Name,
                     ShiftStartTime = pt.ShiftStartTime,
                     EarlyClockInBufferMin = pt.EarlyClockInBufferMin,
                 },
                 ((pageNumber - 1) * pageSize),
                 pageSize,
                 null,
                 orderExpression,
                 isOrderDescending
                    );
            } else
            {
                (profileTemplates, totalRecords) = await hrmsDbContext.ProfileTemplate.FindWithPaginationAsync(
                    pt => new ProfileTemplateViewModel 
                    {
                        Id = pt.Id,
                        Unit = $"{pt.Unit.Name} - {pt.Unit.Index}",
                        Role = pt.Role.Name ?? "Default",
                        EmployeeType = pt.EmployeeType.Name,
                        ShiftStartTime = pt.ShiftStartTime,
                        EarlyClockInBufferMin = pt.EarlyClockInBufferMin,
                    },
                    ((pageNumber - 1) * pageSize),
                    pageSize,
                    pt => pt.Unit.DepartmentId == departmentId
                    );
            }

            return new PagedResponse<ProfileTemplateViewModel>(profileTemplates, pageNumber, pageSize, totalRecords, orderBy, isOrderDescending);
        }

        public async Task<ProfileTemplate?> UpdateProfileTemplateASync(int id, ProfileTemplateViewModel pvm)
        {
            int count = await hrmsDbContext.ProfileTemplate.Where(pt => pt.RoleId == pvm.RoleId && pt.EmployeeTypeId == pvm.EmployeeTypeId && pt.PayFrequencyId == pvm.PayFrequencyId && pt.UnitId == pvm.UnitId).CountAsync();

            if (count > 1)
            {
                throw new ArgumentException("Profile template with the combination of the role, employee type, pay frequency and unit already exists");
            }

            var profileTemplate = await hrmsDbContext.ProfileTemplate.FindOrThrowAsync(id);
            hrmsDbContext.Entry(profileTemplate).CurrentValues.SetValues(new {
                pvm.EarlyClockInBufferMin,
                pvm.ShiftStartTime,
                pvm.RoleId,
                pvm.EmployeeTypeId,
                pvm.PayFrequencyId,
                pvm.UnitId,
            });
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
