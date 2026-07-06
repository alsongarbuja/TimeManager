using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
//using System.Security.Claims;
using TimeManager.Backend.Controllers.PunchManagement.Dto;
using TimeManager.Backend.Data;
using TimeManager.Backend.Models.Punch_Management;

namespace TimeManager.Backend.Controllers.PunchManagement
{
    [ApiController]
    [Route("api/[controller]")]
    public class PunchController(HrmsDbContext ctx, ILogger<PunchController> logger) : ControllerBase
    {

        //[Authorize(AuthenticationSchemes = "Kiosk")]
        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult<PunchEntry>> ClockInOut([FromBody] PunchEntryDto punchEntryDto) {
            //var departmentIdClaim = User.FindFirstValue("department_id");
            //if (departmentIdClaim is null || !int.TryParse(departmentIdClaim, out int departmentId))
            //{
            //    return Unauthorized(new { message = "Invalid or missing Kiosk session" });
            //}

            var jobProfile = await ctx.JobProfile.Where(
                jp => jp.Employee.UniqueId == punchEntryDto.UniqueId &&
                    jp.ProfileTemplate.Unit.DepartmentId == punchEntryDto.DepartmentId
                ).Select(jp => new { 
                    id = (int?)jp.Id,
                    earlyBufferMin = jp.ProfileTemplate.EarlyClockInBufferMin,
                    shiftStartTime = jp.ProfileTemplate.ShiftStartTime,
                }).FirstOrDefaultAsync();

            if (jobProfile == null)
            {
                logger.LogWarning("No Job profile found for the given Id");
                return NotFound(new { message = "No job profile found for the given Id" });
            }

            PunchEntry? punchEntry = await ctx.PunchEntry.Where(
                    pe => pe.JobProfileId == jobProfile.id && pe.ClockOut == null
                ).FirstOrDefaultAsync();

            var msg = "";
            bool isClockedOut = false;

            if (punchEntry == null)
            {
                if (IsClockInAllowed(jobProfile.shiftStartTime, jobProfile.earlyBufferMin))
                {
                    ctx.PunchEntry.Add(new PunchEntry
                    {
                        ClockIn = DateTime.UtcNow,
                        JobProfileId = (int)jobProfile.id!
                    });
                    msg = "Succefully clocked in!!";
                }
                else
                {
                    return BadRequest(new { message = "You cannot clock in at this time" });
                }
            } else
            {
                punchEntry.ClockOut = DateTime.UtcNow;
                //ctx.Entry(punchEntry).CurrentValues.SetValues(new
                //{
                //    ClockOut = DateTime.UtcNow
                //});
                msg = "Succefully clocked out!!";
                isClockedOut = true;
            }

            await ctx.SaveChangesAsync();

            return Ok(new { message = msg, isClockedOut });
        }
        private static bool IsClockInAllowed(TimeOnly startShiftTime, int EarlyBufferMin)
        {
            TimeSpan bufferMin = TimeSpan.FromMinutes(EarlyBufferMin);

            TimeOnly earliestAllowed = startShiftTime.Add(-bufferMin);
            TimeOnly currentTime = TimeOnly.FromDateTime(DateTime.Now);

            return currentTime >= earliestAllowed;
        }
    }
}
