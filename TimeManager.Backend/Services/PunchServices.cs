using Microsoft.EntityFrameworkCore;
using TimeManager.Backend.Controllers.PunchManagement.Dto;
using TimeManager.Backend.Data;
using TimeManager.Backend.Extensions;
using TimeManager.Backend.Models.Punch_Management;
using TimeManager.Backend.ViewModels;

namespace TimeManager.Backend.Services
{
    public interface IPunchServices
    {
        Task<PunchViewOverall> GetPunchesAsync(int? departmentId);
        Task<PunchViewModel> GetPunchByIdAsync(int id);
        Task<PunchEntry?> UpdatePunchAsync(int id, PunchDto punchEntryDto);
    }

    public class PunchServices(HrmsDbContext context) : IPunchServices
    {
        public async Task<PunchViewModel> GetPunchByIdAsync(int id)
        {
            var punch = await context.PunchEntry.FindOrThrowAsync(id);
            return new PunchViewModel
            {
                Id = punch.Id,
                ClockInTime = punch.ClockIn,
                ClockOutTime = punch.ClockOut,
                EmployeeId = punch.JobProfileId,
                Name = $"{punch.JobProfile.Employee.FirstName} {punch.JobProfile.Employee.LastName}"
            };
        }

        public async Task<PunchViewOverall> GetPunchesAsync(int? departmentId)
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
                    Name = $"{e.Employee.FirstName} {e.Employee.LastName}"
                }).ToListAsync();
            }

            var punches = await context.PunchEntry.Select(pe => new PunchViewModel
            {
                Id = pe.Id,
                ClockInTime = pe.ClockIn,
                ClockOutTime = pe.ClockOut,
                EmployeeId = pe.JobProfile.Id,
                Name = $"{pe.JobProfile.Employee.FirstName} {pe.JobProfile.Employee.LastName}"
            })
                .OrderByDescending(pe => pe.ClockInTime).ToListAsync();
            return new PunchViewOverall
            {
                employees = employees,
                punches = punches,
            };
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
