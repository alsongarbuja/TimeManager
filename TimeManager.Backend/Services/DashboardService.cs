using Microsoft.AspNetCore.Identity;
using TimeManager.Backend.Common;
using TimeManager.Backend.ViewModels;
using U = TimeManager.Backend.Models.AuthManagement.User;

namespace TimeManager.Backend.Services
{
    public interface IDashboardService
    {
        Task<DashboardViewModel> GetCurrentUserDashboardData(int profileId);
        Task<SuperAdminDashboardSetupViewModel> GetSuperAdminSetupDashboardCheckData();
    }

    public class DashboardService(
        IPunchServices punchServices,
        IReportService reportSerice,
        UserManager<U> userManager,
        IConfiguration configuration
        ) : IDashboardService
    {
        public async Task<DashboardViewModel> GetCurrentUserDashboardData(int profileId)
        {
            var currentPunchStatus = await punchServices.GetCurrentUserPunchStauts(profileId);
            var currentPayPeriodPunches = await reportSerice.GenerateReportByJobProfileId(profileId);

            return new DashboardViewModel { 
                PunchStatus = currentPunchStatus,
                TotalHours = currentPayPeriodPunches?.TotalHours ?? 0.0,
                WeekOne = currentPayPeriodPunches?.WeekOne ?? [],
                WeekTwo = currentPayPeriodPunches?.WeekTwo ?? []
            };
        }

        public async Task<SuperAdminDashboardSetupViewModel> GetSuperAdminSetupDashboardCheckData()
        {
            bool hasDefaultSuperAdmin = false;
            var users = await userManager.GetUsersInRoleAsync(AppConstants.SUPER_ADMIN_ROLE);
            bool hasNewSuperAdmin = users.Count > 1;

            foreach (var user in users)
            {
                if (user.Email == configuration["SeedSettings:SuperAdminEmail"])
                {
                    hasDefaultSuperAdmin = true;
                    break;
                }
            }

            return new SuperAdminDashboardSetupViewModel
            {
                HasDefaultSuperAdmin = hasDefaultSuperAdmin,
                HasNewSuperAdmin = hasNewSuperAdmin,
            };
        }
    }
}
