using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using TimeManager.Backend.Data;

namespace TimeManager.Backend.Services
{
    public interface IJobProfileService
    {
        Task<IEnumerable<JobProfileData>> GetJobProfilesAsync();
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

        public async Task<IEnumerable<JobProfileData>> GetJobProfilesAsync()
        {
            var jobprofiles = await _context.JobProfile.Select(jp =>
            new JobProfileData {
                Id = jp.Id,
                EmployeeId = jp.EmployeeId,
                ProfileTemplateId = jp.ProfileTemplateId,
                EmployeeString = $"{jp.Employee.FirstName} {jp.Employee.LastName}",
                ProfileTemplateString = $"{jp.ProfileTemplate.Unit.Name} / {jp.ProfileTemplate.Role.Name}",
            }).ToListAsync();
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
    }

    public class JobProfileData
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Profile template Id is required")]
        public int ProfileTemplateId { get; set; }

        [Required(ErrorMessage = "Employee Id is required")]
        public int EmployeeId { get; set; }

        public string ProfileTemplateString { get; set; } = string.Empty;
        public string EmployeeString { get; set; } = string.Empty;
    }
}
