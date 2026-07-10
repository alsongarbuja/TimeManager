using TimeManager.Backend.Data;
using TimeManager.Backend.ViewModels;

namespace TimeManager.Backend.Services
{
    public interface IDashboardService
    {
        Task<DashboardViewModel> GetCurrentUserDashboardData(int profileId);
    }

    public class DashboardService(
        IPunchServices punchServices,
        IReportService reportSerice
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
    }
}
