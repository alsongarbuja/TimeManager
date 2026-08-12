using Microsoft.EntityFrameworkCore;
using TimeManager.Backend.Data;
using TimeManager.Backend.Models.Employee_Management;

namespace TimeManager.Backend.Services
{
    public interface IUserDepartmentPivotService
    {
        Task<IEnumerable<int>> GetUserIdsByDepartmentId(int departmentId);
        Task<IEnumerable<int>> GetDepartmentIdsByUserId(int userId);

        Task AddUserToDepartmentAsync(int userId, int departmentId);
        Task AddUserToDepartmentRangeAsync(List<UserDepartmentPivot> upds);
    }

    public class UserDepartmentPivotService(
        HrmsDbContext context
    ) : IUserDepartmentPivotService
    {
        public async Task AddUserToDepartmentAsync(int userId, int departmentId)
        {
            await context.UserDepartmentPivots.AddAsync(new UserDepartmentPivot
            {
                UserId = userId,
                DepartmentId = departmentId
            });
            await context.SaveChangesAsync();
        }

        public async Task AddUserToDepartmentRangeAsync(List<UserDepartmentPivot> upds)
        {
            await context.UserDepartmentPivots.AddRangeAsync(upds);
            await context.SaveChangesAsync();
        }

        public async Task<IEnumerable<int>> GetDepartmentIdsByUserId(int userId)
        {
            var deptIds = await context.UserDepartmentPivots.Where(
                p => p.UserId == userId).Select(u => u.DepartmentId).ToListAsync();
            return deptIds;
        }

        public async Task<IEnumerable<int>> GetUserIdsByDepartmentId(int departmentId)
        {
            var userIds = await context.UserDepartmentPivots.Where(
                    udp => udp.DepartmentId == departmentId
                ).Select(udp => udp.UserId).ToListAsync();

            return userIds;
        }
    }
}
