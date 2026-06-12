using Microsoft.EntityFrameworkCore;
using TimeManager.Backend.Data;
using TimeManager.Backend.Models.Organization_Management;

namespace TimeManager.Backend.Services
{
    public interface IDepartmentService
    {
        Task<IEnumerable<Department>> GetDepartmentsAsync();
    }

    public class DepartmentService: IDepartmentService
    {
        private readonly HrmsDbContext _context;

        public DepartmentService(HrmsDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Department>> GetDepartmentsAsync()
        {
            var data = await _context.Department.ToListAsync();
            return data;
        }
    }
}
