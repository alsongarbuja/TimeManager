using Microsoft.EntityFrameworkCore;
using TimeManager.Backend.Data;
using TimeManager.Backend.Extensions;
using TimeManager.Backend.Models.Employee_Management;
using TimeManager.Backend.ViewModels.PartialViews;

namespace TimeManager.Backend.Services
{
    public interface IJobHistoryService
    {
        Task<List<JobHistory>> GetJobHistoriesByProfileId(int id);
        Task CreateJobHistoryForProfileId(int id, JobHistoryRowViewModel jhrvm);
        Task<JobHistory?> UpdateJobHistoryById(int jobHistoryId, JobHistoryRowViewModel jhrvm);
        Task<int?> DeleteJobHistoryById(int jobHistoryId);
    }

    public class JobHistoryService(HrmsDbContext context, ILogger<JobProfile> logger) : IJobHistoryService
    {
        public async Task CreateJobHistoryForProfileId(int id, JobHistoryRowViewModel jhrvm)
        {
            context.JobHistory.Add(new JobHistory
            {
                JobProfileId = id,
                ProfileTemplateId = jhrvm.ProfileTemplateId,
                JoinDate = jhrvm.JoinDate.ToUniversalTime(),
                EndDate = jhrvm.EndDate?.ToUniversalTime(),
            });
            await context.SaveChangesAsync();
        }

        public async Task<int?> DeleteJobHistoryById(int jobHistoryId)
        {
            var jobHistory = await context.JobHistory.FindOrThrowAsync(jobHistoryId);
            context.JobHistory.Remove(jobHistory);
            await context.SaveChangesAsync();
            return jobHistoryId;
        }

        public async Task<JobHistory?> UpdateJobHistoryById(int jobHistoryId, JobHistoryRowViewModel jhrvm)
        {
            var jh = await context.JobHistory.FindAsync(jobHistoryId);
            if (jh == null)
            {
                logger.LogWarning($"No job history with id: {jobHistoryId} found");

                return null;
            }

            jh.ProfileTemplateId = jhrvm.ProfileTemplateId;
            jh.JoinDate = jhrvm.JoinDate.ToUniversalTime();
            jh.EndDate = jhrvm.EndDate?.ToUniversalTime();
            
            await context.SaveChangesAsync();
            return jh;
        }

        public async Task<List<JobHistory>> GetJobHistoriesByProfileId(int id)
        {
            var data = await context.JobHistory.Where(jh => jh.JobProfileId == id).OrderBy(jh => jh.JoinDate).ToListAsync();
            return data;
        }
    }
}
