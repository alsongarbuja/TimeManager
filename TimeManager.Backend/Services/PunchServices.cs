using Microsoft.EntityFrameworkCore;
using TimeManager.Backend.Controllers.PunchManagement.Dto;
using TimeManager.Backend.Data;
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

    public class PunchServices : IPunchServices
    {
        private readonly HrmsDbContext context;

        public PunchServices(HrmsDbContext context)
        {
            this.context = context;
        }

        public async Task<PunchViewModel> GetPunchByIdAsync(int id)
        {
            var punch = await context.PunchEntry.Where(pe => pe.Id == id).Select(pe => new PunchViewModel
            {
                Id = pe.Id,
                ClockInTime = pe.ClockIn,
                ClockOutTime = pe.ClockOut,
                EmployeeId = pe.JobProfileId,
                Name = $"{pe.JobProfile.Employee.FirstName} {pe.JobProfile.Employee.LastName}"
            }).FirstOrDefaultAsync();
            return punch;
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
            var p = await context.PunchEntry.FindAsync(id);
            if (p == null) return null;

            context.Entry(p).CurrentValues.SetValues(new { 
                ClockIn = punchEntryDto.ClockIn.ToUniversalTime(),
                ClockOut = punchEntryDto.ClockOut != null ? punchEntryDto.ClockOut?.ToUniversalTime() : null,
            });
            await context.SaveChangesAsync();

            return p;
        }
    }
}
