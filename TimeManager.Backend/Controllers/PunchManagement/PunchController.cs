using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimeManager.Backend.Controllers.PunchManagement.Dto;
using TimeManager.Backend.Data;
using TimeManager.Backend.Models.Punch_Management;

namespace TimeManager.Backend.Controllers.PunchManagement
{
    [ApiController]
    [Route("api/[controller]")]
    public class PunchController: ControllerBase
    {
        private readonly HrmsDbContext _context;
        private readonly ILogger<PunchController> _logger;

        public PunchController(HrmsDbContext ctx, ILogger<PunchController> logger)
        {
            _context = ctx;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult<PunchEntry>> ClockInOut([FromBody] PunchEntryDto punchEntryDto) {
            var jobProfile = await _context.JobProfile.Where(
                jp => jp.Employee.UniqueId == punchEntryDto.UniqueId &&
                    jp.ProfileTemplate.Unit.DepartmentId == punchEntryDto.DepartmentId
                ).Select(jp => new { 
                    id = (int?)jp.Id,
                    earlyBufferMin = jp.ProfileTemplate.EarlyClockInBufferMin,
                    shiftStartTime = jp.ProfileTemplate.ShiftStartTime,
                }).FirstOrDefaultAsync();

            if (jobProfile == null)
            {
                return NotFound(new { message = "No job profile found for the given Id" });
            }

            PunchEntry? punchEntry = await _context.PunchEntry.Where(
                    pe => pe.JobProfileId == jobProfile.id && pe.ClockOut == null
                ).FirstOrDefaultAsync();

            var msg = "";

            if (punchEntry != null)
            {
                _context.Entry(punchEntry).CurrentValues.SetValues(new { 
                    ClockOut = DateTime.UtcNow
                });
                msg = "Succefully clocked out!!";
            } else
            {
                if (IsClockInAllowed(jobProfile.shiftStartTime, jobProfile.earlyBufferMin))
                {
                    _context.PunchEntry.Add(new PunchEntry {
                        ClockIn = DateTime.UtcNow,
                        JobProfileId = (int)jobProfile.id
                    });
                    msg = "Succefully clocked in!!";
                } else
                {
                    return BadRequest(new { message = "You cannot clock in at this time" });
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = msg });
        }
        private bool IsClockInAllowed(TimeOnly startShiftTime, int EarlyBufferMin)
        {
            TimeSpan bufferMin = TimeSpan.FromMinutes(EarlyBufferMin);

            TimeOnly earliestAllowed = startShiftTime.Add(-bufferMin);
            TimeOnly currentTime = TimeOnly.FromDateTime(DateTime.Now);

            return currentTime >= earliestAllowed;
        }
    }
}
