using Microsoft.AspNetCore.Mvc.Rendering;
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
        Task<int> CreateEmployeeAsync(EmployeeDto employeeDto);
        Task<Employee?> UpdateEmployeeAsync(int id, EmployeeDto employeeDto);
        Task<int?> DeleteEmployeeByIdAsync(int id);
        Task<IEnumerable<SelectListItem>> GetEmployeeOptionAsync();
    }

    public class EmployeeService : IEmployeeService
    {
        private readonly HrmsDbContext hrmsDbContext;

        public EmployeeService(HrmsDbContext hrmsDbContext)
        {
            this.hrmsDbContext = hrmsDbContext;
        }

        public async Task<int> CreateEmployeeAsync(EmployeeDto employeeDto)
        {
            Employee employee = new Employee
            {
                FirstName = employeeDto.FirstName,
                LastName = employeeDto.LastName,
                UniqueId = employeeDto.UniqueId,
                UserId = employeeDto.UserId,
                DepartmentId = employeeDto.DepartmentId,
            };
            hrmsDbContext.Employee.Add(employee);
            await this.hrmsDbContext.SaveChangesAsync();
            return employee.Id;
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

        public async Task<IEnumerable<SelectListItem>> GetEmployeeOptionAsync()
        {
            var employees = await this.hrmsDbContext.Employee.Select(e => new SelectListItem
            {
                Text = $"{e.FirstName} {e.LastName}",
                Value = e.Id.ToString()
            }).ToListAsync();
            return employees;
        }

        public async Task<IEnumerable<EmployeeViewModel>> GetEmployeesAsync()
        {
            var employees = await this.hrmsDbContext.Employee.Select(e => new EmployeeViewModel { 
                Id = e.Id,
                FirstName = e.FirstName, 
                LastName = e.LastName,
                Email = e.User.Email,
                UniqueId = e.UniqueId,
                DepartmentName = e.Department.Name,
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
