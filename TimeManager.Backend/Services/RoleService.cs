using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using TimeManager.Backend.Common;
using TimeManager.Backend.Data;
using TimeManager.Backend.Extensions;
using TimeManager.Backend.Models.AuthManagement;
using TimeManager.Backend.ViewModels;

namespace TimeManager.Backend.Services
{
    public interface IRoleService
    {
        Task<IEnumerable<RoleViewModel>> GetRolesAsync();
        Task<RoleViewModel> GetRoleByIdAsync(int id);
        Task<RoleViewModel> GetRoleByNameAsync(string name);
        Task CreateRoleAsync(RoleViewModel rvm);
        Task<RoleViewModel?> UpdateRoleAsync(int id, RoleViewModel rvm);
        Task<int?> DeleteRoleByIdAsync(int id);
        Task<IEnumerable<SelectListItem>> GetRoleOptionsAsync(int selectedId = 0);
    }

    public class RoleService(HrmsDbContext hrmsDbContext, ILogger<Role> logger) : IRoleService
    {
        public async Task<IEnumerable<RoleViewModel>> GetRolesAsync()
        {
            var roles = await hrmsDbContext.Roles
                .Where(r => r.Name != AppConstants.SUPER_ADMIN_ROLE)
                .Select(r => new RoleViewModel
                {
                    Id = r.Id,
                    Name = r.Name ?? "Default",
                })
                .ToListAsync();
            return roles;
        }

        public async Task<RoleViewModel> GetRoleByIdAsync(int id)
        {
            Role role = await hrmsDbContext.Roles.FindOrThrowAsync(id);
            return new RoleViewModel
            {
                Id = role.Id,
                Name = role.Name ?? "",
                Description = role.Description
            };
        }

        public async Task<RoleViewModel> GetRoleByNameAsync(string name)
        {
            Role role = await hrmsDbContext.Roles.WhereOrThrowAsync(r => r.Name == name);
            return new RoleViewModel { 
                Id = role.Id,
                Name = role.Name ?? "",
                Description = role.Description
            };
        }

        public async Task CreateRoleAsync(RoleViewModel rvm)
        {
            hrmsDbContext.Roles.Add(new Role { Name = rvm.Name, Description = rvm.Description });
            await hrmsDbContext.SaveChangesAsync();
        }

        public async Task<RoleViewModel?> UpdateRoleAsync(int id, RoleViewModel rvm)
        {
            var r = await hrmsDbContext.Roles.FindAsync(id);
            if (r == null)
            {
                logger.LogInformation($"Role with id: {id} not found");
                return null;
            }

            hrmsDbContext.Entry(r).CurrentValues.SetValues(rvm);
            await hrmsDbContext.SaveChangesAsync();
            return new RoleViewModel
            {
                Id = r.Id,
                Name = r.Name ?? "",
                Description = r.Description
            };
        }

        public async Task<int?> DeleteRoleByIdAsync(int id)
        {
            var r = await hrmsDbContext.Roles.FindOrThrowAsync(id);
            hrmsDbContext.Roles.Remove(r);
            await hrmsDbContext.SaveChangesAsync();
            return id;
        }

        public async Task<IEnumerable<SelectListItem>> GetRoleOptionsAsync(int selectedId = 0)
        {
            var roles = await hrmsDbContext.Roles
                .Where(r => r.Name != AppConstants.SUPER_ADMIN_ROLE)
                .Select(r => new SelectListItem {
                    Text = r.Name,
                    Value = r.Id.ToString(),
                    Selected = r.Id == selectedId,
                }).ToListAsync();
            return roles;
        }
    }
}
