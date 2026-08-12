using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using TimeManager.Backend.Data;
using TimeManager.Backend.Extensions;
using TimeManager.Backend.Models.Employee_Management;
using TimeManager.Backend.Models.Requests;
using TimeManager.Backend.Models.Responses;
using TimeManager.Backend.Utility;
using TimeManager.Backend.ViewModels;

namespace TimeManager.Backend.Services
{
    public interface IJobProfileService
    {
        Task<PagedResponse<JobProfileViewModel>> GetJobProfilesAsync(
            int? departmentId, 
            PaginationQuery query,
            FilterCondition filter,
            PaginationQuery defaultQuery
            );
        Task<JobProfile> GetJobProfileByIdAsync(int id);
        Task<int?> CreateJobProfileAsync(JobProfileViewModel jpvm);
        Task<JobProfile?> UpdateJobProfileASync(int id, JobProfileViewModel jpvm);
        Task<int?> DeleteJobProfileAsync(int id);
        Task<IEnumerable<SelectListItem>> GetUserOptionsAsync(int? departmentId);
    }

    public class JobProfileService(HrmsDbContext context, ILogger<JobProfile> logger) : IJobProfileService
    {
        public async Task<int?> CreateJobProfileAsync(JobProfileViewModel jpvm)
        {
            var jp = new JobProfile
            {
                EmployeeId = jpvm.EmployeeId,
                ProfileTemplateId = jpvm.ProfileTemplateId,
                EarlyBuffer = jpvm.EarlyBuffer,
            };
            context.JobProfile.Add(jp);
            await context.SaveChangesAsync();

            return jp.Id;
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

        public async Task<PagedResponse<JobProfileViewModel>> GetJobProfilesAsync(
            int? departmentId, 
            PaginationQuery query,
            FilterCondition filter,
            PaginationQuery defaultQuery
            )
        {
            (int pageNumber, int pageSize, string? orderBy, bool isOrderDescending) = PaginationValidation.ConvertToValidPaginationQueries(query, defaultQuery);
            int totalRecords = 0;
            Expression<Func<JobProfile, object>>? orderExpression = orderBy?.ToLower() switch
            {
                "employee" => jp => jp.Employee.FirstName,
                "profile group" => jp => jp.ProfileTemplate.Unit.Name,
                _ => null
            };

            var builder = new ExpressionBuilder<JobProfile>();
            var whereExpression = string.IsNullOrEmpty(filter.Value) 
                ? departmentId == null 
                    ? null 
                    : builder.BuildPredicate(new FilterCondition
                        {
                            PropertyName = "ProfileTemplate.Unit.DepartmentId",
                            Value = departmentId.ToString(),
                            Operator = FilterOperator.Equals
                        })
                : departmentId == null
                    ? builder.BuildPredicate(filter)
                    : builder.BuildPredicate([
                        new FilterCondition { 
                            PropertyName = "ProfileTemplate.Unit.DepartmentId",
                            Value = departmentId.ToString(),
                            Operator = FilterOperator.Equals
                        },
                        filter,
                      ]);

            IEnumerable<JobProfileViewModel> jobprofiles = [];

                (jobprofiles, totalRecords) = await context.JobProfile.FindWithPaginationAsync(
                     jp =>
                    new JobProfileViewModel
                    {
                        Id = jp.Id,
                        EmployeeId = jp.EmployeeId,
                        EmployeeString = $"{jp.Employee.FirstName} {jp.Employee.LastName}",
                        ProfileTemplateString = $"{jp.ProfileTemplate.Unit.Name} ({jp.ProfileTemplate.Unit.Index}) / {jp.ProfileTemplate.Role.Name}",
                        EarlyBuffer = jp.EarlyBuffer,
                        ShiftStartTime = jp.ProfileTemplate.ShiftStartTime,
                    },
                    ((pageNumber - 1) * pageSize),
                    pageSize,
                    whereExpression,
                    orderExpression,
                    isOrderDescending
                  );

            return new PagedResponse<JobProfileViewModel>(jobprofiles, pageNumber, pageSize, totalRecords, orderBy, isOrderDescending);
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
            if (jp == null)
            {
                logger.LogWarning($"No job profile with id: {id} found");

                return null;
            }

            jp.EarlyBuffer = jpvm.EarlyBuffer;
            jp.ProfileTemplateId = jpvm.ProfileTemplateId;
            jp.EmployeeId = jpvm.EmployeeId;

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

        [Required(ErrorMessage = "Join Date is required")]
        public DateTime JoinDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string ProfileTemplateString { get; set; } = string.Empty;
        public string EmployeeString { get; set; } = string.Empty;
    }
}
