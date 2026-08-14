using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TimeManager.Backend.Data;
using TimeManager.Backend.Extensions;
using TimeManager.Backend.Models.Organization_Management;
using TimeManager.Backend.ViewModels;

namespace TimeManager.Backend.Services
{
    public interface IDepartmentService
    {
        Task<IEnumerable<DepartmentViewModel>> GetDepartmentsAsync();
        Task<DepartmentViewModel> GetDepartmentByIdAsync(int id);
        Task CreateDepartmentAsync(DepartmentViewModel dvm);
        Task<DepartmentViewModel?> UpdateDepartmentAsync(int id, DepartmentViewModel dvm);
        Task<int?> DeleteDepartmentByIdAsync(int id);
        Task<IEnumerable<SelectListItem>> GetDepartmentOptionsAsync(int selectedId = 0);
        Task<IEnumerable<SelectListItem>> GetDepartmentOptionsMultiAsync(IEnumerable<int> selectedIds);
    }

    public class DepartmentService(HrmsDbContext context, ILogger<Department> logger) : IDepartmentService
    {
        public async Task<IEnumerable<DepartmentViewModel>> GetDepartmentsAsync()
        {
            var data = await context.Department.Select(d => new DepartmentViewModel
            {
                Id = d.Id,
                Name = d.Name,
                Description = d.Description,
            }).ToListAsync();
            return data;
        }

        public async Task<DepartmentViewModel> GetDepartmentByIdAsync(int id)
        {
            var department = await context.Department.FindOrThrowAsync(id);
            return new DepartmentViewModel
            {
                Id = department.Id,
                Name = department.Name,
                Description = department.Description
            };
        }


        public async Task CreateDepartmentAsync(DepartmentViewModel dvm)
        {
            context.Department.Add(new Department { Name = dvm.Name, Description = dvm.Description });
            await context.SaveChangesAsync();
        }

        public async Task<DepartmentViewModel?> UpdateDepartmentAsync(int id, DepartmentViewModel dvm)
        {
            var dept = await context.Department.FindAsync(id);
            if (dept == null)
            {
                logger.LogInformation($"Department not found for id: {id}");
                return null;
            }

            context.Entry(dept).CurrentValues.SetValues(dvm);
            await context.SaveChangesAsync();

            return new DepartmentViewModel
            {
                Id = dept.Id,
                Name = dept.Name,
                Description = dept.Description
            };
        }

        public async Task<int?> DeleteDepartmentByIdAsync(int id)
        {
            var dept = await context.Department.FindOrThrowAsync(id);
            context.Department.Remove(dept);
            await context.SaveChangesAsync();
            return id;
        }
        
        public async Task<IEnumerable<SelectListItem>> GetDepartmentOptionsAsync(int selectedItem = 0)
        {
            var data = await context.Department.Select(d => new SelectListItem
            {
                Text = d.Name,
                Value = d.Id.ToString(),
                Selected = d.Id == selectedItem,
            }).ToListAsync();
            return data;
        }

        public async Task<IEnumerable<SelectListItem>> GetDepartmentOptionsMultiAsync(IEnumerable<int> selectedIds)
        {
            var data = await context.Department.Select(d => new SelectListItem
            {
                Text = d.Name,
                Value = d.Id.ToString(),
                Selected = selectedIds.Contains(d.Id),
            }).ToListAsync();
            return data;
        }
    }
}
