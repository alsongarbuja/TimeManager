using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
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
        Task<PagedResponse<EmployeeViewModel>> GetEmployeesAsync(int? departmentId, PaginationQuery filter);
        Task<Employee> GetEmployeeByIdAsync(int id);
        Task<int> CreateEmployeeAsync(EmployeeDto employeeDto);
        Task<Employee?> UpdateEmployeeAsync(int id, EmployeeDto employeeDto);
        Task<int?> DeleteEmployeeByIdAsync(int id);
        Task<IEnumerable<SelectListItem>> GetEmployeeOptionAsync(int selectedId = 0);
        Task<Employee?> GetEmployeeByUserIdAsync(int id);
        Task<IEnumerable<JobProfile>> GetJobProfilesByUserIdAsync(int id);
    }

    public class EmployeeService(
        HrmsDbContext hrmsDbContext, 
        ILogger<Employee> logger
        ) : IEmployeeService
    {
        public async Task<int> CreateEmployeeAsync(EmployeeDto employeeDto)
        {
            bool exists = await hrmsDbContext.Employee.AnyAsync(e => e.UniqueId == employeeDto.UniqueId);

            if (exists) {
                throw new ArgumentException("The unqiue Id is already registered to an employee");
            }

            try
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
            } catch (Exception ex)
            {
                throw new Exception("Database operation failed", ex);
            }
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

        public async Task<PagedResponse<EmployeeViewModel>> GetEmployeesAsync(int? departmentId, PaginationQuery filter)
        {
            (int pageNumber, int pageSize, string? orderBy, bool isOrderDescending) = PaginationValidation.ConvertToValidPaginationQueries(filter);
            Expression<Func<Employee, object>>? orderExpression = orderBy?.ToLower() switch
            {
                "so id" => e => e.UniqueId,
                "name" => e => e.FirstName,
                "email" => e => e.User.Email,
                _ => null
            };
            
            int totalRecords = 0;
            IEnumerable<EmployeeViewModel> employees = [];

            if (departmentId == null)
            {
                logger.LogInformation("No department Id found sending back all users");
                (employees, totalRecords) = await hrmsDbContext.Employee.FindWithPaginationAsync(
                    e => new EmployeeViewModel
                    {
                        Id = e.Id,
                        FirstName = e.FirstName,
                        LastName = e.LastName,
                        Email = e.User.Email ?? string.Empty,
                        UniqueId = e.UniqueId,
                    },
                    ((pageNumber - 1) * pageSize),
                    pageSize,
                    null,
                    orderExpression,
                    isOrderDescending
                 );
            } else
            {
                logger.LogInformation("Sending only the department connected users");
                var excludedUserIds = await hrmsDbContext.UserRoles
                    .Join(hrmsDbContext.Roles,
                        ur => ur.RoleId,
                        r => r.Id,
                        (ur, r) => new { ur.UserId, r.Name })
                    .Where(x => x.Name == AppConstants.SUPER_ADMIN_ROLE || x.Name == AppConstants.ADMIN_ROLE)
                    .Select(x => x.UserId)
                    .ToHashSetAsync();
                var excludedOtherDepartmentUserIds = await hrmsDbContext.JobProfile.Where(
                        jp => jp.ProfileTemplate.Unit.DepartmentId != departmentId
                    ).Select(jp => jp.Employee.UserId).ToHashSetAsync();

                (employees, totalRecords) = await hrmsDbContext.Employee.FindWithPaginationAsync(
                    e => new EmployeeViewModel
                    {
                        Id = e.Id,
                        FirstName = e.FirstName,
                        LastName = e.LastName,
                        Email = e.User.Email ?? string.Empty,
                        UniqueId = e.UniqueId,
                    },
                    ((pageNumber - 1) * pageSize),
                    pageSize,
                    e => !excludedUserIds.Contains(e.UserId) && !excludedOtherDepartmentUserIds.Contains(e.UserId)
                );
            }
            return new PagedResponse<EmployeeViewModel>(employees, pageNumber, pageSize, totalRecords, orderBy, isOrderDescending);
        }

        public async Task<IEnumerable<JobProfile>> GetJobProfilesByUserIdAsync(int id)
        {
            var employee = await hrmsDbContext.Employee.Where(e => e.UserId == id).FirstOrDefaultAsync();
            if (employee == null)
            {
                logger.LogWarning($"No employee found with user id: {id}");
                return [];
            }

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
            if (e == null) { 
                logger.LogWarning($"No employee found with user id: {id}");
                return null; 
            }

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

        public int? DepartmentId { get; set; }
    }
}
