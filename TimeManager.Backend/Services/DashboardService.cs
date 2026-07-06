using TimeManager.Backend.Data;
using TimeManager.Backend.ViewModels;

namespace TimeManager.Backend.Services
{
    public interface IDashboardService
    {
        Task<DashboardViewModel> GetCurrentUserDashboardData(int profileId);
    }

    public class DashboardService(IPunchServices punchServices) : IDashboardService
    {
        public async Task<DashboardViewModel> GetCurrentUserDashboardData(int profileId)
        {
            var currentPunchStatus = await punchServices.GetCurrentUserPunchStauts(profileId);

            return new DashboardViewModel { 
                PunchStatus = currentPunchStatus,
            };
        }
    }
}
