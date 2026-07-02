using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using TimeManager.Backend.Common;
using TimeManager.Backend.Data;
using TimeManager.Backend.Extensions;
using TimeManager.Backend.Models.Employee_Management;
using TimeManager.Backend.Models.Requests;
using TimeManager.Backend.Models.Responses;
using TimeManager.Backend.Utility;
using TimeManager.Backend.ViewModels;

namespace TimeManager.Backend.Services
{
    public interface IEmployeeService
    {
        Task<PagedResponse<EmployeeViewModel>> GetEmployeesAsync(int? departmentId, PaginationFilter filter);
        Task<Employee> GetEmployeeByIdAsync(int id);
        Task<int> CreateEmployeeAsync(EmployeeDto employeeDto);
        Task<Employee?> UpdateEmployeeAsync(int id, EmployeeDto employeeDto);
        Task<int?> DeleteEmployeeByIdAsync(int id);
        Task<IEnumerable<SelectListItem>> GetEmployeeOptionAsync(int selectedId = 0);
        Task<Employee?> GetEmployeeByUserIdAsync(int id);
        Task<IEnumerable<JobProfile>> GetJobProfilesByUserIdAsync(int id);
    }

    public class EmployeeService(HrmsDbContext hrmsDbContext) : IEmployeeService
    {
        public async Task<int> CreateEmployeeAsync(EmployeeDto employeeDto)
        {
            Employee employee = new()
            {
                FirstName = employeeDto.FirstName,
                LastName = employeeDto.LastName,
                UniqueId = employeeDto.UniqueId,
                UserId = employeeDto.UserId,
                DepartmentId = employeeDto.DepartmentId,
            };
            hrmsDbContext.Employee.Add(employee);
            await hrmsDbContext.SaveChangesAsync();
            return employee.Id;
        }

        public async Task<int?> DeleteEmployeeByIdAsync(int id)
        {
            var e = await hrmsDbContext.Employee.FindOrThrowAsync(id);
            hrmsDbContext.Employee.Remove(e);
            await hrmsDbContext.SaveChangesAsync();
            return id;
        }

        public async Task<Employee> GetEmployeeByIdAsync(int id)
        {
            return await hrmsDbContext.Employee.FindOrThrowAsync(id);
        }

        public async Task<Employee?> GetEmployeeByUserIdAsync(int id)
        {
            var employee = await hrmsDbContext.Employee.Where(e => e.UserId == id).FirstOrDefaultAsync();
            return employee;
        }

        public async Task<IEnumerable<SelectListItem>> GetEmployeeOptionAsync(int selectedId = 0)
        {
            var employees = await hrmsDbContext.Employee.Select(e => new SelectListItem
            {
                Text = $"{e.FirstName} {e.LastName}",
                Value = e.Id.ToString(),
                Selected = e.Id == selectedId,
            }).ToListAsync();
            return employees;
        }

        public async Task<PagedResponse<EmployeeViewModel>> GetEmployeesAsync(int? departmentId, PaginationFilter filter)
        {
            var query = hrmsDbContext.Employee.AsNoTracking().AsQueryable();
            int totalRecords = 0;

            (int pageNumber, int pageSize) = PaginationValidation.ValidateFilterValues(filter);

            IEnumerable<EmployeeViewModel> employees = [];
            if (departmentId == null)
            {
                totalRecords = await query.CountAsync();
                employees = await query.Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize).Select(e => new EmployeeViewModel { 
                    Id = e.Id,
                    FirstName = e.FirstName, 
                    LastName = e.LastName,
                    Email = e.User.Email ?? string.Empty,
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
                    .Where(x => x.Name == AppConstants.SUPER_ADMIN_ROLE || x.Name == AppConstants.ADMIN_ROLE)
                    .Select(x => x.UserId)
                    .ToHashSetAsync();

                totalRecords = await query.Where(e => e.DepartmentId == departmentId && !excludedUserIds.Contains(e.UserId)).CountAsync();

                employees = await query
                    .Where(e => e.DepartmentId == departmentId && !excludedUserIds.Contains(e.UserId))
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(e => new EmployeeViewModel
                        {
                            Id = e.Id,
                            FirstName = e.FirstName,
                            LastName = e.LastName,
                            Email = e.User.Email ?? string.Empty,
                            UniqueId = e.UniqueId,
                            DepartmentName = e.Department.Name,
                        }).ToListAsync();
            }
            return new PagedResponse<EmployeeViewModel>(employees, pageNumber, pageSize, totalRecords);
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
            var e = await hrmsDbContext.Employee.FindAsync(id);
            if (e == null) return null;

            hrmsDbContext.Entry(e).CurrentValues.SetValues(employeeDto);
            await hrmsDbContext.SaveChangesAsync();
            return e;
        }
    }

    public class EmployeeDto
    {
        [Required(ErrorMessage = "Unique Id is required")]
        [StringLength(20, ErrorMessage = "Unique Id cannot exceed 20 characters")]
        public string UniqueId { get; set; } = string.Empty;

        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [StringLength(50, ErrorMessage = "Email cannot exceed 50 characters")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "User id is required")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Department Id is required")]
        public int DepartmentId { get; set; }
    }
}
