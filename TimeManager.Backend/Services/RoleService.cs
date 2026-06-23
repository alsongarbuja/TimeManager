using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TimeManager.Backend.Controllers.EmployeeManagement.Dto;
using TimeManager.Backend.Data;
using TimeManager.Backend.Models.AuthManagement;
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
        Task<IEnumerable<SelectListItem>> GetRoleOptionsAsync(string selectedItem = "");
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
            this.hrmsDbContext.Roles.Add(new Role { Name = roleDto.Name, Description = roleDto.Description });
            await this.hrmsDbContext.SaveChangesAsync();
        }

        public async Task<int?> DeleteRoleByIdAsync(int id)
        {
            var r = await this.hrmsDbContext.Roles.FindAsync(id);
            if (r == null) return null;

            this.hrmsDbContext.Roles.Remove(r);
            await this.hrmsDbContext.SaveChangesAsync();
            return id;
        }

        public async Task<Role> GetRoleByIdAsync(int id)
        {
            var r = await this.hrmsDbContext.Roles.FindAsync(id);
            return r;
        }

        public async Task<IEnumerable<SelectListItem>> GetRoleOptionsAsync(string selectedItem = "")
        {
            var roles = await hrmsDbContext.Roles
                .Where(r => r.Name != "SuperAdmin")
                .Select(r => new SelectListItem {
                    Text = r.Name,
                    Value = r.Name,
                    Selected = r.Name == selectedItem,
                }).ToListAsync();
            return roles;
        }

        public async Task<IEnumerable<RoleViewModel>> GetRolesAsync()
        {
            var roles = await this.hrmsDbContext.Roles
                .Where(r => r.Name != "SuperAdmin")
                .Select(r => new RoleViewModel
                    {
                        Id = r.Id,
                        Name = r.Name ?? "Default",
                    })
                .ToListAsync();
            return roles;
        }

        public async Task<Role?> UpdateRoleAsync(int id, RoleDto roleDto)
        {
            var r = await this.hrmsDbContext.Roles.FindAsync(id);
            if (r == null) return null;

            this.hrmsDbContext.Entry(r).CurrentValues.SetValues(roleDto);
            await this.hrmsDbContext.SaveChangesAsync();
            return r;
        }
    }
}
