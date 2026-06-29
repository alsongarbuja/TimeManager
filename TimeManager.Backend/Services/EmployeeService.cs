using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TimeManager.Backend.Controllers.Employee;
using TimeManager.Backend.Data;
using TimeManager.Backend.Models.Employee_Management;
using TimeManager.Backend.ViewModels;

namespace TimeManager.Backend.Services
{
    public interface IEmployeeService
    {
        Task<IEnumerable<EmployeeViewModel>> GetEmployeesAsync(int? departmentId);
        Task<Employee> GetEmployeeByIdAsync(int id);
        Task<int> CreateEmployeeAsync(EmployeeDto employeeDto);
        Task<Employee?> UpdateEmployeeAsync(int id, EmployeeDto employeeDto);
        Task<int?> DeleteEmployeeByIdAsync(int id);
        Task<IEnumerable<SelectListItem>> GetEmployeeOptionAsync();
        Task<Employee?> GetEmployeeByUserIdAsync(int id);
        Task<IEnumerable<JobProfile>> GetJobProfilesByUserIdAsync(int id);
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

        public async Task<Employee?> GetEmployeeByUserIdAsync(int id)
        {
            var employee = await hrmsDbContext.Employee.Where(e => e.UserId == id).FirstOrDefaultAsync();
            return employee;
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

        public async Task<IEnumerable<EmployeeViewModel>> GetEmployeesAsync(int? departmentId)
        {
            IEnumerable<EmployeeViewModel> employees = [];
            if (departmentId == null)
            {
                employees = await this.hrmsDbContext.Employee.Select(e => new EmployeeViewModel { 
                    Id = e.Id,
                    FirstName = e.FirstName, 
                    LastName = e.LastName,
                    Email = e.User.Email,
                    UniqueId = e.UniqueId,
                    DepartmentName = e.Department.Name,
                }).ToListAsync();
            } else
            {
                var excludedUserIds = await hrmsDbContext.UserRoles
                    .Join(hrmsDbContext.Roles,
                        ur => ur.RoleId,
                        r => r.Id,
                        (ur, r) => new { ur.UserId, r.Name })
                    .Where(x => x.Name == "SuperAdmin" || x.Name == "Admin")
                    .Select(x => x.UserId)
                    .ToHashSetAsync();

                employees = await this.hrmsDbContext.Employee
                    .Where(e => e.DepartmentId == departmentId && !excludedUserIds.Contains(e.UserId))
                    .Select(e => new EmployeeViewModel
                        {
                            Id = e.Id,
                            FirstName = e.FirstName,
                            LastName = e.LastName,
                            Email = e.User.Email,
                            UniqueId = e.UniqueId,
                            DepartmentName = e.Department.Name,
                        }).ToListAsync();
            }
            return employees;
        }

        public async Task<IEnumerable<JobProfile>> GetJobProfilesByUserIdAsync(int id)
        {
            var employee = await hrmsDbContext.Employee.Where(e => e.UserId == id).FirstOrDefaultAsync();
            if (employee == null) return [];

            var jobProfiles = await hrmsDbContext.JobProfile
                .Include(jp => jp.ProfileTemplate)
                    .ThenInclude(pt => pt.Unit)
                    .ThenInclude(u => u.Department)
                .Include(jp => jp.ProfileTemplate)
                    .ThenInclude(pt => pt.Role)
                .Where(jp => jp.EmployeeId == employee.Id).ToListAsync();
            return jobProfiles;
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
