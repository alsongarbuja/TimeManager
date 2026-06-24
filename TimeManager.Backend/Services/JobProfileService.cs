using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TimeManager.Backend.Data;
using TimeManager.Backend.Models.Employee_Management;
using TimeManager.Backend.ViewModels;

namespace TimeManager.Backend.Services
{
    public interface IJobProfileService
    {
        Task<IEnumerable<JobProfileViewModel>> GetJobProfilesAsync(int? departmentId);
        Task<JobProfile> GetJobProfileByIdAsync(int id);
        Task CreateJobProfileAsync(JobProfileViewModel jpvm);
        Task<JobProfile?> UpdateJobProfileASync(int id, JobProfileViewModel jpvm);
        Task<int?> DeleteJobProfileAsync(int id);
        Task<IEnumerable<SelectListItem>> GetUserOptionsAsync();
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

        public async Task CreateJobProfileAsync(JobProfileViewModel jpvm)
        {
            _context.JobProfile.Add(new JobProfile { EmployeeId = jpvm.EmployeeId, ProfileTemplateId = jpvm.ProfileTemplateId });
            await _context.SaveChangesAsync();
        }

        public async Task<int?> DeleteJobProfileAsync(int id)
        {
            var jp = await _context.JobProfile.FindAsync(id);
            if (jp == null) return null;
            _context.JobProfile.Remove(jp);
            await _context.SaveChangesAsync();
            return id;
        }

        public async Task<JobProfile> GetJobProfileByIdAsync(int id)
        {
            var jp = await _context.JobProfile.FindAsync(id);
            return jp;
        }

        public async Task<IEnumerable<JobProfileViewModel>> GetJobProfilesAsync(int? departmentId)
        {
            IEnumerable<JobProfileViewModel> jobprofiles = [];
            
            if (departmentId == null)
            {
                jobprofiles = await _context.JobProfile.Select(jp =>
                new JobProfileViewModel {
                    Id = jp.Id,
                    EmployeeId = jp.EmployeeId,
                    EmployeeString = $"{jp.Employee.FirstName} {jp.Employee.LastName}",
                    ProfileTemplateString = $"{jp.ProfileTemplate.Unit.Name} / {jp.ProfileTemplate.Role.Name}",
                }).ToListAsync();
            } else
            {
                jobprofiles = await _context.JobProfile
                    .Where(jp => jp.ProfileTemplate.Unit.DepartmentId == departmentId)
                    .Select(jp =>
                new JobProfileViewModel
                {
                    Id = jp.Id,
                    EmployeeId = jp.EmployeeId,
                    EmployeeString = $"{jp.Employee.FirstName} {jp.Employee.LastName}",
                    ProfileTemplateString = $"{jp.ProfileTemplate.Unit.Name} / {jp.ProfileTemplate.Role.Name}",
                }).ToListAsync();
            }

            return jobprofiles;
        }

        public async Task<IEnumerable<SelectListItem>> GetUserOptionsAsync()
        {
            var users = await _context.JobProfile.Select(jp => new SelectListItem
            {
                Value = jp.Id.ToString(),
                Text = $"{jp.Employee.FirstName} {jp.Employee.LastName}",
            }).ToListAsync();

            return users;
        }

        public async Task<JobProfile?> UpdateJobProfileASync(int id, JobProfileViewModel jpvm)
        {
            var jp = await _context.JobProfile.FindAsync(id);
            if (jp == null) return null;
            _context.Entry(jp).CurrentValues.SetValues(jpvm);
            await _context.SaveChangesAsync();
            return jp;
        }
    }
}
