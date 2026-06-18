using Microsoft.EntityFrameworkCore;
using TimeManager.Backend.Controllers.EmployeeManagement.Dto;
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
    }

    public class EmployeeTypeService : IEmployeeTypeService
    {
        private readonly HrmsDbContext hrmsDbContext;

        public EmployeeTypeService(HrmsDbContext hrmsDbContext)
        {
            this.hrmsDbContext = hrmsDbContext;
        }

        public async Task CreateEmployeeTypeAsync(EmployeeTypeDto employeeTypeDto)
        {
            this.hrmsDbContext.EmployeeType.Add(new EmployeeType {
                Name = employeeTypeDto.Name,
                Description = employeeTypeDto.Description
            });
            await this.hrmsDbContext.SaveChangesAsync();
        }

        public async Task<int?> DeleteEmployeeTypeByIdAsync(int id)
        {
            var et = await this.hrmsDbContext.EmployeeType.FindAsync(id);
            if (et == null) return null;

            this.hrmsDbContext.EmployeeType.Remove(et);
            await this.hrmsDbContext.SaveChangesAsync();
            return id;
        }

        public async Task<EmployeeType> GetEmployeeTypeByIdAsync(int id)
        {
            var et = await this.hrmsDbContext.EmployeeType.FindAsync(id);
            return et;
        }

        public async Task<EmployeeType?> UpdateEmployeeTypeAsync(int id, EmployeeTypeDto employeeTypeDto)
        {
            var et = await this.hrmsDbContext.EmployeeType.FindAsync(id);
            if (et == null) return null;

            this.hrmsDbContext.Entry(et).CurrentValues.SetValues(employeeTypeDto);
            await this.hrmsDbContext.SaveChangesAsync();
            return et;
        }

        public async Task<IEnumerable<EmployeeTypeViewModel>> GetEmployeeTypesAsync()
        {
            var ets = await this.hrmsDbContext.EmployeeType.Select(et => new EmployeeTypeViewModel { 
                Id = et.Id,
                Name = et.Name,
                Description = et.Description,
            }).ToListAsync();
            return ets;
        }

        public async Task<EmployeeType> GetEmployeeTypeByNameAsync(string name)
        {
            var et = await hrmsDbContext.EmployeeType.Where(et => et.Name.Equals(name)).FirstAsync();
            return et;
        }
    }
}
