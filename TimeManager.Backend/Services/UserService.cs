using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TimeManager.Backend.Common;
using TimeManager.Backend.Data;
using TimeManager.Backend.Models.AuthManagement;
using TimeManager.Backend.Models.Requests;
using TimeManager.Backend.Models.Responses;
using TimeManager.Backend.Utility;
using TimeManager.Backend.ViewModels;

namespace TimeManager.Backend.Services
{
    public interface IUserService
    {
        Task<PagedResponse<UserViewModel>> GetUsersAsync(PaginationFilter filter);
        Task<(User? User, Role? Role)> GetUserByIdAsync(int id);
        //Task CreateUserAsync(UserViewModel uvm);
        Task<User?> UpdateUserAsync(int id, RegisterViewModel rvm);
        Task<int?> DeleteUserByIdAsync(int id);
        Task<IEnumerable<SelectListItem>> GetUserOptionsAsync(int selectedId = 0);
    }

    public class UserService(
        HrmsDbContext hrmsDbContext,
        IHttpContextAccessor httpContextAccessor,
        UserManager<User> userManager,
        IRoleService roleService,
        IConfiguration configuration,
        ILogger<User> logger
        ) : IUserService
    {
        public async Task<int?> DeleteUserByIdAsync(int id)
        {
            var user = await userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                logger.LogWarning($"User with id: {id} not found");
                return null;
            }
            var DeleteResult = await userManager.DeleteAsync(user);
            if (!DeleteResult.Succeeded) {
                logger.LogError(DeleteResult.Errors.First().Description);
                return null;
            }
            return id;
        }

        public async Task<(User? User, Role? Role)> GetUserByIdAsync(int id)
        {
            var user = await userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                logger.LogWarning($"User with id: {id} not found");
                return (null, null);
            }

            var roles = await userManager.GetRolesAsync(user);
            var role = await roleService.GetRoleByNameAsync(roles[0]);

            return (user, role);
        }

        public async Task<IEnumerable<SelectListItem>> GetUserOptionsAsync(int selectedId = 0)
        {
            var currUser = await userManager.GetUserAsync(httpContextAccessor.HttpContext!.User);
            var currUserRole = await userManager.GetRolesAsync(currUser!);

            var isSuperUser = currUserRole.Contains(AppConstants.SUPER_ADMIN_ROLE);

            IEnumerable<SelectListItem> users = [];

            if (isSuperUser)
            {
                users = await hrmsDbContext.Users
                .Where(u => !hrmsDbContext.UserRoles
                .Join(hrmsDbContext.Roles, ur => ur.RoleId, r => r.Id,
                    (ur, r) => new { ur.UserId, r.Name })
                .Any(ur => ur.UserId == u.Id && ur.Name == AppConstants.SUPER_ADMIN_ROLE))
                .Select(u => new SelectListItem
                {
                    Text = u.UserName,
                    Value = u.Id.ToString(),
                    Selected = u.Id == selectedId,
                })
                .ToListAsync();
            } else
            {
                users = await hrmsDbContext.Users
                    .Where(u => !hrmsDbContext.UserRoles
                    .Join(hrmsDbContext.Roles, ur => ur.RoleId, r => r.Id, 
                        (ur, r) => new { ur.UserId, r.Name })
                    .Any(ur => ur.UserId == u.Id && (ur.Name == AppConstants.SUPER_ADMIN_ROLE || ur.Name == "Admin")))
                    .Select(u => new SelectListItem
                        {
                            Text = u.UserName,
                            Value = u.Id.ToString(),
                            Selected = u.Id == selectedId,
                        })
                    .ToListAsync();
            }

            return users;
        }

        public async Task<PagedResponse<UserViewModel>> GetUsersAsync(PaginationFilter filter)
        {
            (int pageNumber, int pageSize) = PaginationValidation.ValidateFilterValues(filter);

            var query = hrmsDbContext.Users.AsNoTracking().AsQueryable();
            int totalRecords = await query.Where(u => !hrmsDbContext.UserRoles.
                    Join(hrmsDbContext.Roles,
                        ur => ur.RoleId,
                        r => r.Id,
                        (ur, r) => new { ur.UserId, r.Name })
                    .Any(ur => ur.UserId == u.Id && ur.Name == AppConstants.SUPER_ADMIN_ROLE)).CountAsync();

            IEnumerable<UserViewModel> users = [];
            users = await query
                .Where(u => !hrmsDbContext.UserRoles.
                    Join(hrmsDbContext.Roles, 
                        ur => ur.RoleId, 
                        r => r.Id, 
                        (ur, r) => new { ur.UserId, r.Name })
                    .Any(ur => ur.UserId == u.Id && ur.Name == AppConstants.SUPER_ADMIN_ROLE))
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(u => new UserViewModel
                        {
                            Id = u.Id,
                            UserName = u.UserName,
                            Email = u.Email,
                        }).ToListAsync();
            return new PagedResponse<UserViewModel>(users, pageNumber, pageSize, totalRecords);
        }

        public async Task<User?> UpdateUserAsync(int id, RegisterViewModel rvm)
        {
            var u = await userManager.FindByIdAsync(id.ToString());
            if (u == null)
            {
                logger.LogWarning($"User with id: {id} not found");
                return null;
            }

            u.Email = rvm.Email;
            u.UserName = rvm.Email.Split("@")[0];

            var updatedUser = await userManager.UpdateAsync(u);
            if (!updatedUser.Succeeded)
            {
                logger.LogError(updatedUser.Errors.First().Description);
                return null;
            }

            var role = await roleService.GetRoleByIdAsync(rvm.Role);
            var currentRoles = await userManager.GetRolesAsync(u);
            if (currentRoles.Count < 1 || currentRoles[0] != role.Name)
            {
                await userManager.AddToRoleAsync(u, role.Name!);
            }

            if (!string.IsNullOrEmpty(rvm.Password))
            {
                await userManager.RemovePasswordAsync(u);
                var passwordUpdated = await userManager.AddPasswordAsync(u, rvm.Password);

                if (!passwordUpdated.Succeeded)
                {
                    logger.LogError(passwordUpdated.Errors.First().Description);
                    return null;
                }
            }
            
            return u;
        }
    }
}
