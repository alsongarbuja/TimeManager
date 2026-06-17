using Microsoft.EntityFrameworkCore;
using TimeManager.Backend.Controllers.EmployeeManagement.Dto;
using TimeManager.Backend.Data;
using TimeManager.Backend.Models.Employee_Management;
using TimeManager.Backend.ViewModels;

namespace TimeManager.Backend.Services
{
    public interface IEmployeeService
    {
        Task<IEnumerable<EmployeeViewModel>> GetEmployeesAsync();
        Task<Employee> GetEmployeeByIdAsync(int id);
        Task CreateEmployeeAsync(EmployeeDto employeeDto);
        Task<Employee?> UpdateEmployeeAsync(int id, EmployeeDto employeeDto);
        Task<int?> DeleteEmployeeByIdAsync(int id);
    }

    public class EmployeeService : IEmployeeService
    {
        private readonly HrmsDbContext hrmsDbContext;

        public EmployeeService(HrmsDbContext hrmsDbContext)
        {
            this.hrmsDbContext = hrmsDbContext;
        }

        public async Task CreateEmployeeAsync(EmployeeDto employeeDto)
        {
            this.hrmsDbContext.Employee.Add(new Employee {
                Email = employeeDto.Email,
                FirstName = employeeDto.FirstName,
                LastName = employeeDto.LastName,
                UniqueId = employeeDto.UniqueId,
            });
            await this.hrmsDbContext.SaveChangesAsync();
        }

        public async Task<int?> DeleteEmployeeByIdAsync(int id)
        {
            var e = await this.hrmsDbContext.Employee.FindAsync(id);
            if (e == null) return null;

            this.hrmsDbContext.Employee.Remove(e);
            await this.hrmsDbContext.SaveChangesAsync();
            return id;
        }

        public async Task<Employee> GetEmployeeByIdAsync(int id)
        {
            var e = await this.hrmsDbContext.Employee.FindAsync(id);
            return e;
        }

        public async Task<IEnumerable<EmployeeViewModel>> GetEmployeesAsync()
        {
            var employees = await this.hrmsDbContext.Employee.Select(e => new EmployeeViewModel { 
                Id = e.Id,
                FirstName = e.FirstName, 
                LastName = e.LastName,
                Email = e.Email,
                UniqueId = e.UniqueId,
            }).ToListAsync();
            return employees;
        }

        public async Task<Employee?> UpdateEmployeeAsync(int id, EmployeeDto employeeDto)
        {
            var e = await this.hrmsDbContext.Employee.FindAsync(id);
            if (e == null) return null;

            this.hrmsDbContext.Entry(e).CurrentValues.SetValues(employeeDto);
            await this.hrmsDbContext.SaveChangesAsync();
            return e;
        }
    }
}
