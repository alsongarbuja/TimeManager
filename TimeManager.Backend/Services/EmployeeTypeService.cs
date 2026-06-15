using Microsoft.EntityFrameworkCore;
using TimeManager.Backend.Data;
using TimeManager.Backend.Models.Employee_Management;

namespace TimeManager.Backend.Services
{
    public interface IEmployeeTypeService
    {
        Task<IEnumerable<EmployeeType>> GetEmployeeTypesAsync();
    }

    public class EmployeeTypeService : IEmployeeTypeService
    {
        private readonly HrmsDbContext hrmsDbContext;

        public EmployeeTypeService(HrmsDbContext hrmsDbContext)
        {
            this.hrmsDbContext = hrmsDbContext;
        }

        public async Task<IEnumerable<EmployeeType>> GetEmployeeTypesAsync()
        {
            var employeeTypes = await this.hrmsDbContext.EmployeeType.ToListAsync();
            return employeeTypes;
        }
    }
}
