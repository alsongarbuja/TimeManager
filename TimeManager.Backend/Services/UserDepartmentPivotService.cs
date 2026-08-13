using Microsoft.EntityFrameworkCore;
using TimeManager.Backend.Data;
using TimeManager.Backend.Models.Employee_Management;

namespace TimeManager.Backend.Services
{
    public interface IUserDepartmentPivotService
    {
        Task<IEnumerable<int>> GetUserIdsByDepartmentId(int departmentId);
        Task<IEnumerable<int>> GetDepartmentIdsByUserId(int userId);

        Task AddUserToDepartmentAsync(int userId, int departmentId, bool checkAlreadyAdd = false);
        Task<bool> CheckAlreadyExistsAsync(int userId, int departmentId);
        Task AddUserToDepartmentRangeAsync(List<UserDepartmentPivot> upds);

        Task<bool> UpdateUserDepartmentAsync(int userId, List<int> departmentIds);
    }

    public class UserDepartmentPivotService(
        HrmsDbContext context
    ) : IUserDepartmentPivotService
    {
        public async Task AddUserToDepartmentAsync(int userId, int departmentId, bool checkAlreadyAdd = false)
        {
            if (checkAlreadyAdd)
            {
                bool userRelExists = await CheckAlreadyExistsAsync(userId, departmentId);
                if (userRelExists)
                    return;
            }
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

        public async Task<bool> CheckAlreadyExistsAsync(int userId, int departmentId)
        {
            return await context.UserDepartmentPivots.AnyAsync(u => u.UserId == userId && u.DepartmentId == departmentId);
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

        public async Task<bool> UpdateUserDepartmentAsync(int userId, List<int> departmentIds)
        {
            try
            {
                List<UserDepartmentPivot> oldPivotsDeptIds = await context.UserDepartmentPivots.Where(u => u.UserId == userId && departmentIds.Contains(u.DepartmentId)).ToListAsync();
                List<int> oldDeptIds = [];

                foreach (var oldId in oldPivotsDeptIds)
                {
                    if (departmentIds.Contains(oldId.DepartmentId))
                    {
                        _ = oldDeptIds.Append(oldId.DepartmentId);
                        continue;
                    }
                    context.UserDepartmentPivots.Remove(oldId);
                }

                foreach (var newId in departmentIds)
                {
                    if (!oldDeptIds.Contains(newId))
                    {
                        await AddUserToDepartmentAsync(userId, newId);
                    }
                }

                await context.SaveChangesAsync();
                return true;
            } catch
            {
                return false;
            }
        }
    }
}
