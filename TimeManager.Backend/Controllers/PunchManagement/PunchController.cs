using ClosedXML.Parser;
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

            //var jobProfile = await ctx.JobProfile.Where(
            //    jp => jp.Employee.UniqueId == punchEntryDto.UniqueId &&
            //        jp.ProfileTemplate.Unit.DepartmentId == punchEntryDto.DepartmentId
            //    ).Select(jp => new { 
            //        id = (int?)jp.Id,
            //        earlyBufferMin = jp.ProfileTemplate.EarlyClockInBufferMin,
            //        shiftStartTime = jp.ProfileTemplate.ShiftStartTime,
            //    }).FirstOrDefaultAsync();
            var jobProfile = await GetJobProfileQuery(ctx, punchEntryDto.UniqueId, punchEntryDto.DepartmentId);

            if (jobProfile == null)
            {
                logger.LogWarning("No Job profile found for the given Id");
                return NotFound(new { message = "No job profile found for the given Id" });
            }

            PunchEntry? punchEntry = await ctx.PunchEntry.Where(
                    pe => pe.JobProfileId == jobProfile.Id && pe.ClockOut == null
                ).FirstOrDefaultAsync();

            var msg = "";
            bool isClockedOut = false;

            if (punchEntry == null)
            {
                logger.LogInformation("No punch entry were found with clock out null");

                logger.LogInformation("Trying to clock in the employee");

                logger.LogInformation("Checking if they are within clock in buffer time");
                if (IsClockInAllowed(jobProfile.ShiftStartTime, jobProfile.EarlyBufferMin))
                {
                    logger.LogInformation("Clock in was successful");
                    ctx.PunchEntry.Add(new PunchEntry
                    {
                        ClockIn = DateTime.UtcNow,
                        JobProfileId = (int)jobProfile.Id!
                    });
                    msg = "Succefully clocked in!!";
                }
                else
                {
                    logger.LogWarning("Clock in rejected due to trying to clock in too early");
                    return BadRequest(new { message = $"You cannot clock in at this time. Your shift starts on {jobProfile.ShiftStartTime} and you can clock in starting {jobProfile.EarlyBufferMin} min before" });
                }
            } else
            {
                logger.LogInformation("Clock out was successful");
                punchEntry.ClockOut = DateTime.UtcNow;
                msg = "Succefully clocked out!!";
                isClockedOut = true;
            }

            await ctx.SaveChangesAsync();

            return Ok(new { message = msg, isClockedOut });
        }


        private static readonly Func<HrmsDbContext, string, int, Task<JobProfileProjection?>> GetJobProfileQuery =
    EF.CompileAsyncQuery((HrmsDbContext ctx, string uniqueId, int deptId) =>
        ctx.JobProfile
            .Where(jp => jp.Employee.UniqueId == uniqueId && jp.ProfileTemplate.Unit.DepartmentId == deptId)
            .Select(jp => new JobProfileProjection
            {
                Id = jp.Id,
                EarlyBufferMin = jp.ProfileTemplate.EarlyClockInBufferMin,
                ShiftStartTime = jp.ProfileTemplate.ShiftStartTime
            })
            .FirstOrDefault());

        public class JobProfileProjection
        {
            public int Id { get; set; }
            public int EarlyBufferMin { get; set; }
            public TimeOnly ShiftStartTime { get; set; }
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
