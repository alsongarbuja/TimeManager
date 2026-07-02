using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using TimeManager.Backend.Data;
using TimeManager.Backend.Extensions;
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
        Task<IEnumerable<SelectListItem>> GetUserOptionsAsync(int? departmentId);
    }

    public class JobProfileService(HrmsDbContext context) : IJobProfileService
    {
        public async Task CreateJobProfileAsync(JobProfileViewModel jpvm)
        {
            context.JobProfile.Add(new JobProfile { EmployeeId = jpvm.EmployeeId, ProfileTemplateId = jpvm.ProfileTemplateId });
            await context.SaveChangesAsync();
        }

        public async Task<int?> DeleteJobProfileAsync(int id)
        {
            var jp = await context.JobProfile.FindOrThrowAsync(id);
            context.JobProfile.Remove(jp);
            await context.SaveChangesAsync();
            return id;
        }

        public async Task<JobProfile> GetJobProfileByIdAsync(int id)
        {
            return await context.JobProfile.FindOrThrowAsync(id);
        }

        public async Task<IEnumerable<JobProfileViewModel>> GetJobProfilesAsync(int? departmentId)
        {
            IEnumerable<JobProfileViewModel> jobprofiles = [];
            
            if (departmentId == null)
            {
                jobprofiles = await context.JobProfile.Select(jp =>
                new JobProfileViewModel {
                    Id = jp.Id,
                    EmployeeId = jp.EmployeeId,
                    EmployeeString = $"{jp.Employee.FirstName} {jp.Employee.LastName}",
                    ProfileTemplateString = $"{jp.ProfileTemplate.Unit.Name} ({jp.ProfileTemplate.Unit.Index}) / {jp.ProfileTemplate.Role.Name}",
                }).ToListAsync();
            } else
            {
                jobprofiles = await context.JobProfile
                    .Where(jp => jp.ProfileTemplate.Unit.DepartmentId == departmentId)
                    .Select(jp =>
                new JobProfileViewModel
                {
                    Id = jp.Id,
                    EmployeeId = jp.EmployeeId,
                    EmployeeString = $"{jp.Employee.FirstName} {jp.Employee.LastName}",
                    ProfileTemplateString = $"{jp.ProfileTemplate.Unit.Name} ({jp.ProfileTemplate.Unit.Index})/ {jp.ProfileTemplate.Role.Name}",
                }).ToListAsync();
            }

            return jobprofiles;
        }

        public async Task<IEnumerable<SelectListItem>> GetUserOptionsAsync(int? departmentId)
        {
            IEnumerable<SelectListItem> users = [];

            if (departmentId == null)
            {
                users = await context.JobProfile.Select(jp => new SelectListItem
                {
                    Value = jp.Id.ToString(),
                    Text = $"{jp.Employee.FirstName} {jp.Employee.LastName} / {jp.ProfileTemplate.Unit.Name}",
                }).ToListAsync();
            } else
            {
                users = await context.JobProfile
                    .Where(jp => jp.ProfileTemplate.Unit.DepartmentId == departmentId)
                    .Select(jp => new SelectListItem
                        {
                            Value = jp.Id.ToString(),
                            Text = $"{jp.Employee.FirstName} {jp.Employee.LastName}",
                        }).ToListAsync();
            }

            return users;
        }

        public async Task<JobProfile?> UpdateJobProfileASync(int id, JobProfileViewModel jpvm)
        {
            var jp = await context.JobProfile.FindAsync(id);
            if (jp == null) return null;
            context.Entry(jp).CurrentValues.SetValues(jpvm);
            await context.SaveChangesAsync();
            return jp;
        }
    }

    public class JobProfileDto
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
