using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TimeManager.Backend.Data;
using TimeManager.Backend.Models.Employee_Management;

namespace TimeManager.Backend.Services
{
    public class CurrentEmployeeService
    {
        private readonly HrmsDbContext hrmsDbContext;
        private readonly IHttpContextAccessor httpContextAccessor;

        public CurrentEmployeeService(HrmsDbContext hrmsDbContext, IHttpContextAccessor httpContextAccessor)
        {
            this.hrmsDbContext = hrmsDbContext;
            this.httpContextAccessor = httpContextAccessor;
        }

        private int GetCurrentUserId()
        {
            var claim = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException("No authenticated user found");
            return int.Parse(claim);
        }

        public async Task<Employee> GetCurrentEmployeeAsync()
        {
            var userId = GetCurrentUserId();
            var employee = await hrmsDbContext.Employee.Include(e => e.Department).FirstOrDefaultAsync(e => e.UserId == userId);

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
