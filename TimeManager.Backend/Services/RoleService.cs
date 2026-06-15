using Microsoft.EntityFrameworkCore;
using TimeManager.Backend.Data;
using TimeManager.Backend.Models.Employee_Management;

namespace TimeManager.Backend.Services
{
    public interface IRoleService
    {
        Task<IEnumerable<Role>> GetRolesAsync();
    }

    public class RoleService: IRoleService
    {
        private readonly HrmsDbContext hrmsDbContext;

        public RoleService(HrmsDbContext hrmsDbContext)
        {
            this.hrmsDbContext = hrmsDbContext;
        }

        public async Task<IEnumerable<Role>> GetRolesAsync()
        {
            var roles = await this.hrmsDbContext.Role.ToListAsync();
            return roles;
        }
    }
}
