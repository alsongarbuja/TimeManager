using Microsoft.EntityFrameworkCore;
using TimeManager.Backend.Data;
using TimeManager.Backend.Models.Employee_Management;

namespace TimeManager.Backend.Services
{
    public interface IJobProfileService
    {
        Task<IEnumerable<JobProfile>> GetJobProfilesAsync();
    }

    public class JobProfileService: IJobProfileService
    {
        private readonly HrmsDbContext _context;
        private readonly ILogger<JobProfileService> _logger;

        public JobProfileService(HrmsDbContext context, ILogger<JobProfileService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<JobProfile>> GetJobProfilesAsync()
        {
            var jobprofiles = await _context.JobProfile.Select(jp =>
            new JobProfile {
                Id = jp.Id,
                EmployeeId = jp.EmployeeId,
                ProfileTemplateId = jp.ProfileTemplateId,
                Employee = jp.Employee,
            }).ToListAsync();
            return jobprofiles;
        }
    }
}
