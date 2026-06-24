using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TimeManager.Backend.Data;
using TimeManager.Backend.Models.AuthManagement;
using TimeManager.Backend.ViewModels;

namespace TimeManager.Backend.Services
{
    public interface IUserService
    {
        Task<IEnumerable<UserViewModel>> GetUsersAsync();
        Task<(User User, IList<string> Roles)> GetUserByIdAsync(int id);
        //Task CreateUserAsync(UserViewModel uvm);
        Task<User?> UpdateUserAsync(int id, RegisterViewModel rvm);
        Task<int?> DeleteUserByIdAsync(int id);
        Task<IEnumerable<SelectListItem>> GetUserOptionsAsync();
    }

    public class UserService: IUserService
    {
        private readonly HrmsDbContext hrmsDbContext;
        private readonly UserManager<User> userManager;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IConfiguration configuration;

        public UserService(
            HrmsDbContext hrmsDbContext, 
            IHttpContextAccessor httpContextAccessor, 
            UserManager<User> userManager,
            IConfiguration configuration
        )
        {
            this.hrmsDbContext = hrmsDbContext;
            this.httpContextAccessor = httpContextAccessor;
            this.userManager = userManager;
            this.configuration = configuration;
        }

        public async Task<int?> DeleteUserByIdAsync(int id)
        {
            var user = await userManager.FindByIdAsync(id.ToString());
            if (user == null) return null;
            var DeleteResult = await userManager.DeleteAsync(user);
            if (!DeleteResult.Succeeded) {
                return null;
            }
            return id;
        }

        public async Task<(User User, IList<string> Roles)> GetUserByIdAsync(int id)
        {
            var user = await userManager.FindByIdAsync(id.ToString());
            if (user == null) return (null, null);

            var roles = await userManager.GetRolesAsync(user);

            return (user, roles);
        }

        public async Task<IEnumerable<SelectListItem>> GetUserOptionsAsync()
        {
            var currUser = await userManager.GetUserAsync(httpContextAccessor.HttpContext!.User);
            var currUserRole = await userManager.GetRolesAsync(currUser!);

            var superAdminRole = configuration["Auth:SuperAdminRole"] ?? throw new InvalidOperationException("Super admin role must be configured in the env");
            var isSuperUser = currUserRole.Contains(superAdminRole);

            IEnumerable<SelectListItem> users = [];

            if (isSuperUser)
            {
                users = await hrmsDbContext.Users
                .Where(u => !hrmsDbContext.UserRoles
                .Join(hrmsDbContext.Roles, ur => ur.RoleId, r => r.Id,
                    (ur, r) => new { ur.UserId, r.Name })
                .Any(ur => ur.UserId == u.Id && ur.Name == "SuperAdmin"))
                .Select(u => new SelectListItem
                {
                    Text = u.UserName,
                    Value = u.Id.ToString(),
                })
                .ToListAsync();
            } else
            {
                users = await hrmsDbContext.Users
                    .Where(u => !hrmsDbContext.UserRoles
                    .Join(hrmsDbContext.Roles, ur => ur.RoleId, r => r.Id, 
                        (ur, r) => new { ur.UserId, r.Name })
                    .Any(ur => ur.UserId == u.Id && (ur.Name == "SuperAdmin" || ur.Name == "Admin")))
                    .Select(u => new SelectListItem
                        {
                            Text = u.UserName,
                            Value = u.Id.ToString(),
                        })
                    .ToListAsync();
            }

            return users;
        }

        public async Task<IEnumerable<UserViewModel>> GetUsersAsync()
        {
            IEnumerable<UserViewModel> users = [];
            users = await hrmsDbContext.Users
                .Where(u => !hrmsDbContext.UserRoles.
                    Join(hrmsDbContext.Roles, 
                        ur => ur.RoleId, 
                        r => r.Id, 
                        (ur, r) => new { ur.UserId, r.Name })
                    .Any(ur => ur.UserId == u.Id && ur.Name == "SuperAdmin"))
                    .Select(u => new UserViewModel
                        {
                            Id = u.Id,
                            UserName = u.UserName,
                            Email = u.Email,
                        }).ToListAsync();
            return users;
        }

        public async Task<User?> UpdateUserAsync(int id, RegisterViewModel rvm)
        {
            var u = await userManager.FindByIdAsync(id.ToString());
            if (u == null) return null;

            u.Email = rvm.Email;
            u.UserName = rvm.Email.Split("@")[0];

            var updatedUser = await userManager.UpdateAsync(u);
            if (!updatedUser.Succeeded) return null;

            if (!string.IsNullOrEmpty(rvm.Password))
            {
                await userManager.RemovePasswordAsync(u);
                var passwordUpdated = await userManager.AddPasswordAsync(u, rvm.Password);

                if (!passwordUpdated.Succeeded) return null; 
            }
            
            return u;
        }
    }
}
