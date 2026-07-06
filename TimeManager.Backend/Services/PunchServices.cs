using Microsoft.EntityFrameworkCore;
using TimeManager.Backend.Controllers.PunchManagement.Dto;
using TimeManager.Backend.Data;
using TimeManager.Backend.Extensions;
using TimeManager.Backend.Models.Employee_Management;
using TimeManager.Backend.Models.Punch_Management;
using TimeManager.Backend.Models.Requests;
using TimeManager.Backend.Models.Responses;
using TimeManager.Backend.Utility;
using TimeManager.Backend.ViewModels;

namespace TimeManager.Backend.Services
{
    public interface IPunchServices
    {
        Task<PagedResponse<PunchViewModel>> GetPunchesAsync(int? departmentId, PaginationFilter filter);
        Task<PunchEntry?> GetPunchByIdAsync(int id);
        Task<PunchEntry?> UpdatePunchAsync(int id, PunchDto punchEntryDto);
        Task<int?> DeletePunchByIdAsync(int id);

        Task<PunchStatusModel> GetCurrentUserPunchStauts(int profileId);
    }

    public class PunchServices(HrmsDbContext context) : IPunchServices
    {
        public async Task<int?> DeletePunchByIdAsync(int id)
        {
            var punch = await context.PunchEntry.FindOrThrowAsync(id);
            context.PunchEntry.Remove(punch);
            await context.SaveChangesAsync();
            return id;
        }

        public async Task<PunchStatusModel> GetCurrentUserPunchStauts(int profileId)
        {
            DateTime todaysDate = DateTime.UtcNow.Date; 
            var punchesToday = await context.PunchEntry.Where(pe => pe.JobProfileId == profileId && pe.ClockIn > todaysDate).OrderByDescending(pe => pe.ClockIn).ToListAsync();

            if (punchesToday.Count == 0)
            {
                var punchYesterDay = await context.PunchEntry.Where(pe => pe.JobProfileId == profileId).OrderByDescending(pe => pe.ClockIn).FirstOrDefaultAsync();
                
                return new PunchStatusModel
                {
                    IsActive = false,
                    LastTimeStamp = punchYesterDay?.ClockOut?.ToLocalTime()
                };
            }

            bool isCurrentlyActive = true;
            DateTime clockInTimeStamp = punchesToday[^1].ClockIn.ToLocalTime();
            DateTime? clockOutTimeStamp = null;

            if (punchesToday[0].ClockOut == null)
            {
                if (punchesToday.Count > 1)
                {
                    clockOutTimeStamp = punchesToday[1].ClockOut?.ToLocalTime();
                } else
                {
                    clockOutTimeStamp = punchesToday[0].ClockOut?.ToLocalTime();
                }
            } else
            {
                isCurrentlyActive = false;
                clockOutTimeStamp = punchesToday[0].ClockOut?.ToLocalTime();
            }

            return new PunchStatusModel
            {
                IsActive = isCurrentlyActive,
                TimeStamp = clockInTimeStamp,
                LastTimeStamp = clockOutTimeStamp,
            };
        }

        public async Task<PunchEntry?> GetPunchByIdAsync(int id)
        {
             return await context.PunchEntry.Include(p => p.JobProfile).ThenInclude(jp => jp.Employee).Where(p => p.Id == id).FirstOrDefaultAsync();
        }

        public async Task<PagedResponse<PunchViewModel>> GetPunchesAsync(int? departmentId, PaginationFilter filter)
        {
            (int pageNumber, int pageSize) = PaginationValidation.ValidateFilterValues(filter);

            var query = context.PunchEntry.AsNoTracking().AsQueryable();
            int totalRecords = await query.CountAsync();

            var punches = await query
                .OrderByDescending(pe => pe.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(pe => new PunchViewModel
                {
                    Id = pe.Id,
                    ClockInTime = pe.ClockIn,
                    ClockOutTime = pe.ClockOut,
                    EmployeeId = pe.JobProfile.Id,
                    Name = $"{pe.JobProfile.Employee.FirstName} {pe.JobProfile.Employee.LastName}"
                })
                .ToListAsync();
            return new PagedResponse<PunchViewModel>(punches, pageNumber, pageSize, totalRecords);
        }

        public async Task<PunchEntry?> UpdatePunchAsync(int id, PunchDto punchEntryDto)
        {
            var p = await context.PunchEntry.FindOrThrowAsync(id);
            context.Entry(p).CurrentValues.SetValues(new { 
                ClockIn = punchEntryDto.ClockIn.ToUniversalTime(),
                ClockOut = punchEntryDto.ClockOut != null ? punchEntryDto.ClockOut?.ToUniversalTime() : null,
            });
            await context.SaveChangesAsync();

            return p;
        }
    }
}
