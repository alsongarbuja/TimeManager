using Microsoft.EntityFrameworkCore;
using TimeManager.Backend.Data;
using TimeManager.Backend.Models.Employee_Management;

namespace TimeManager.Backend.Services
{
    public interface IEmployeeService
    {
        Task<IEnumerable<Employee>> GetEmployeesAsync();
    }

    public class EmployeeService : IEmployeeService
    {
        private readonly HrmsDbContext hrmsDbContext;

        public EmployeeService(HrmsDbContext hrmsDbContext)
        {
            this.hrmsDbContext = hrmsDbContext;
        }

        public async Task<IEnumerable<Employee>> GetEmployeesAsync()
        {
            var employees = await this.hrmsDbContext.Employee.ToListAsync();
            return employees;
        }
    }
}
