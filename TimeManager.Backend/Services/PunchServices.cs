using Microsoft.EntityFrameworkCore;
using TimeManager.Backend.Controllers.PunchManagement.Dto;
using TimeManager.Backend.Data;
using TimeManager.Backend.Extensions;
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

        public async Task<PunchEntry?> GetPunchByIdAsync(int id)
        {
             return await context.PunchEntry.Include(p => p.JobProfile).ThenInclude(jp => jp.Employee).Where(p => p.Id == id).FirstOrDefaultAsync();
        }

        public async Task<PagedResponse<PunchViewModel>> GetPunchesAsync(int? departmentId, PaginationFilter filter)
        {
            IEnumerable<Employees> employees = [];

            if (departmentId == null)
            {
                employees = await context.JobProfile.Select(e => new Employees
                {
                    Id = e.Id,
                    Name = $"{e.Employee.FirstName} {e.Employee.LastName} / {e.ProfileTemplate.Unit.Name}"
                }).ToListAsync();
            } else
            {
                employees = await context.JobProfile.Where(e => e.ProfileTemplate.Unit.Department.Id == departmentId).Select(e => new Employees
                {
                    Id = e.Id,
                    Name = $"{e.Employee.FirstName} {e.Employee.LastName} / {e.ProfileTemplate.Unit.Name}"
                }).ToListAsync();
            }

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
