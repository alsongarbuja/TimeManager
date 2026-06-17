using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TimeManager.Backend.Data;
using TimeManager.Backend.Models.Organization_Management;
using TimeManager.Backend.ViewModels;

namespace TimeManager.Backend.Services
{
    public interface IUserService
    {
        Task<IEnumerable<UserViewModel>> GetUsersAsync();
        Task<IdentityUser> GetUserByIdAsync(int id);
        Task CreateUserAsync(UserViewModel uvm);
        Task<IdentityUser?> UpdateUserAsync(int id, UserViewModel uvm);
        Task<int?> DeleteUserByIdAsync(int id);
    }

    public class UserService: IUserService
    {
        private readonly HrmsDbContext hrmsDbContext;

        public UserService(HrmsDbContext hrmsDbContext)
        {
            this.hrmsDbContext = hrmsDbContext;
        }

        public Task CreateUserAsync(UserViewModel uvm)
        {
            throw new NotImplementedException();
        }

        public Task<int?> DeleteUserByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityUser> GetUserByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<UserViewModel>> GetUsersAsync()
        {
            var users = await hrmsDbContext.Users.Select(u => new UserViewModel
            {
                Id = u.Id,
                UserName = u.UserName,
                Email = u.Email,
            }).ToListAsync();
            return users;
        }

        public Task<IdentityUser?> UpdateUserAsync(int id, UserViewModel uvm)
        {
            throw new NotImplementedException();
        }
    }
}
