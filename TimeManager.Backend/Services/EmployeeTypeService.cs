using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using TimeManager.Backend.Data;
using TimeManager.Backend.Models.Employee_Management;
using TimeManager.Backend.ViewModels;

namespace TimeManager.Backend.Services
{
    public interface IEmployeeTypeService
    {
        Task<IEnumerable<EmployeeTypeViewModel>> GetEmployeeTypesAsync();
        Task<EmployeeType> GetEmployeeTypeByIdAsync(int id);
        Task<EmployeeType> GetEmployeeTypeByNameAsync(string name);
        Task CreateEmployeeTypeAsync(EmployeeTypeDto employeeTypeDto);
        Task<EmployeeType?> UpdateEmployeeTypeAsync(int id, EmployeeTypeDto employeeTypeDto);
        Task<int?> DeleteEmployeeTypeByIdAsync(int id);
        Task<IEnumerable<SelectListItem>> GetEmployeeTypeOptionsAsync();
    }

    public class EmployeeTypeService(HrmsDbContext hrmsDbContext) : IEmployeeTypeService
    {
        public async Task CreateEmployeeTypeAsync(EmployeeTypeDto employeeTypeDto)
        {
            hrmsDbContext.EmployeeType.Add(new EmployeeType {
                Name = employeeTypeDto.Name,
                Description = employeeTypeDto.Description
            });
            await hrmsDbContext.SaveChangesAsync();
        }

        public async Task<int?> DeleteEmployeeTypeByIdAsync(int id)
        {
            var et = await hrmsDbContext.EmployeeType.FindAsync(id);
            if (et == null) return null;

            hrmsDbContext.EmployeeType.Remove(et);
            await hrmsDbContext.SaveChangesAsync();
            return id;
        }

        public async Task<EmployeeType> GetEmployeeTypeByIdAsync(int id)
        {
            var et = await hrmsDbContext.EmployeeType.FindAsync(id);
            return et;
        }

        public async Task<IEnumerable<SelectListItem>> GetEmployeeTypeOptionsAsync()
        {
            var roles = await hrmsDbContext.EmployeeType.Select(r => new SelectListItem
            {
                Text = r.Name,
                Value = r.Id.ToString(),
            }).ToListAsync();
            return roles;
        }

        public async Task<EmployeeType?> UpdateEmployeeTypeAsync(int id, EmployeeTypeDto employeeTypeDto)
        {
            var et = await hrmsDbContext.EmployeeType.FindAsync(id);
            if (et == null) return null;

            hrmsDbContext.Entry(et).CurrentValues.SetValues(employeeTypeDto);
            await hrmsDbContext.SaveChangesAsync();
            return et;
        }

        public async Task<IEnumerable<EmployeeTypeViewModel>> GetEmployeeTypesAsync()
        {
            var ets = await hrmsDbContext.EmployeeType.Select(et => new EmployeeTypeViewModel { 
                Id = et.Id,
                Name = et.Name,
                Description = et.Description,
            }).ToListAsync();
            return ets;
        }

        public async Task<EmployeeType> GetEmployeeTypeByNameAsync(string name)
        {
            var et = await hrmsDbContext.EmployeeType.Where(et => et.Name.Equals(name)).FirstOrDefaultAsync();
            return et;
        }
    }

    public class EmployeeTypeDto
    {
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Description cannot exceed 100 characters")]
        public string? Description { get; set; } = string.Empty;
    }
}
