using Microsoft.EntityFrameworkCore;
using TimeManager.Backend.Controllers.EmployeeManagement.Dto;
using TimeManager.Backend.Data;
using TimeManager.Backend.Models.Employee_Management;
using TimeManager.Backend.ViewModels;

namespace TimeManager.Backend.Services
{
    public interface IRoleService
    {
        Task<IEnumerable<RoleViewModel>> GetRolesAsync();
        Task<Role> GetRoleByIdAsync(int id);
        Task CreateRoleAsync(RoleDto roleDto);
        Task<Role?> UpdateRoleAsync(int id, RoleDto roleDto);
        Task<int?> DeleteRoleByIdAsync(int id);
    }

    public class RoleService: IRoleService
    {
        private readonly HrmsDbContext hrmsDbContext;

        public RoleService(HrmsDbContext hrmsDbContext)
        {
            this.hrmsDbContext = hrmsDbContext;
        }

        public async Task CreateRoleAsync(RoleDto roleDto)
        {
            this.hrmsDbContext.Role.Add(new Role { Name = roleDto.Name, Description = roleDto.Description });
            await this.hrmsDbContext.SaveChangesAsync();
        }

        public async Task<int?> DeleteRoleByIdAsync(int id)
        {
            var r = await this.hrmsDbContext.Role.FindAsync(id);
            if (r == null) return null;

            this.hrmsDbContext.Role.Remove(r);
            await this.hrmsDbContext.SaveChangesAsync();
            return id;
        }

        public async Task<Role> GetRoleByIdAsync(int id)
        {
            var r = await this.hrmsDbContext.Role.FindAsync(id);
            return r;
        }

        public async Task<IEnumerable<RoleViewModel>> GetRolesAsync()
        {
            var roles = await this.hrmsDbContext.Role.Select(r => new RoleViewModel
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
            }).ToListAsync();
            return roles;
        }

        public async Task<Role?> UpdateRoleAsync(int id, RoleDto roleDto)
        {
            var r = await this.hrmsDbContext.Role.FindAsync(id);
            if (r == null) return null;

            this.hrmsDbContext.Entry(r).CurrentValues.SetValues(roleDto);
            await this.hrmsDbContext.SaveChangesAsync();
            return r;
        }
    }
}
