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
        Task<Role> GetRoleByIdAsync(int id);
        Task<Role> GetRoleByNameAsync(string name);
        Task CreateRoleAsync(RoleDto roleDto);
        Task<Role?> UpdateRoleAsync(int id, RoleDto roleDto);
        Task<int?> DeleteRoleByIdAsync(int id);
        Task<IEnumerable<SelectListItem>> GetRoleOptionsAsync(int selectedId = 0);
    }

    public class RoleService(HrmsDbContext hrmsDbContext, ILogger<Role> logger) : IRoleService
    {
        public async Task CreateRoleAsync(RoleDto roleDto)
        {
            hrmsDbContext.Roles.Add(new Role { Name = roleDto.Name, Description = roleDto.Description });
            await hrmsDbContext.SaveChangesAsync();
        }

        public async Task<int?> DeleteRoleByIdAsync(int id)
        {
            var r = await hrmsDbContext.Roles.FindOrThrowAsync(id);
            hrmsDbContext.Roles.Remove(r);
            await hrmsDbContext.SaveChangesAsync();
            return id;
        }

        public async Task<Role> GetRoleByIdAsync(int id)
        {
            return await hrmsDbContext.Roles.FindOrThrowAsync(id);
        }

        public async Task<Role> GetRoleByNameAsync(string name)
        {
            return await hrmsDbContext.Roles.WhereOrThrowAsync(r => r.Name == name);
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

        public async Task<Role?> UpdateRoleAsync(int id, RoleDto roleDto)
        {
            var r = await hrmsDbContext.Roles.FindAsync(id);
            if (r == null)
            {
                logger.LogWarning($"Role with id: {id} not found");
                return null;
            }

            hrmsDbContext.Entry(r).CurrentValues.SetValues(roleDto);
            await hrmsDbContext.SaveChangesAsync();
            return r;
        }
    }

    public class RoleDto
    {
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Description cannot exceed 100 characters")]
        public string? Description { get; set; } = string.Empty;
    }
}
