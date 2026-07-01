using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TimeManager.Backend.Data;
using TimeManager.Backend.Models.Employee_Management;

namespace TimeManager.Backend.Services
{
    public class CurrentEmployeeService(HrmsDbContext hrmsDbContext, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
    {
        private int GetCurrentUserId()
        {
            var claim = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException("No authenticated user found");
            return int.Parse(claim);
        }

        public string GetCurrentUserRole()
        {
            var role = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Role) ?? throw new UnauthorizedAccessException("No authenticated user found");
            return role;
        }

        public bool IsCurrentUserSuperAdmin()
        {
            var role = this.GetCurrentUserRole();
            var superAdminRole = configuration["Auth:SuperAdminRole"] ?? throw new InvalidOperationException("Super admin role must be configured in the env");

            return role == superAdminRole;
        }

        public async Task<Employee> GetCurrentEmployeeAsync()
        {
            var userId = GetCurrentUserId();
            var employee = await hrmsDbContext.Employee.Include(e => e.Department).FirstOrDefaultAsync(e => e.UserId == userId) ?? throw new KeyNotFoundException("Current Employee not found for the user id");
            return employee;
        }

        public async Task<int> GetCurrentEmployeeDepartmentIdAsync(int? dpId)
        {
            if (dpId != null) return (int)dpId;
            var employee = await GetCurrentEmployeeAsync();
            return employee.DepartmentId;
        }
    }
}
