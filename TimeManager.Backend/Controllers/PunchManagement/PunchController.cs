using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

using TimeManager.Backend.Controllers.PunchManagement.Dto;
using TimeManager.Backend.Data;
using TimeManager.Backend.Models.Punch_Management;
using TimeManager.Backend.Services;

namespace TimeManager.Backend.Controllers.PunchManagement
{
    [ApiController]
    [Route("api/[controller]")]
    public class PunchController(
        HrmsDbContext ctx, 
        ILogger<PunchController> logger,
        IJobHistoryService jobHistoryService    
    ) : ControllerBase
    {

        [Authorize(AuthenticationSchemes = "Kiosk")]
        //[AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult<PunchEntry>> ClockInOut([FromBody] PunchEntryDto punchEntryDto) {
            var departmentIdClaim = User.FindFirstValue("department_id");
            if (departmentIdClaim is null || !int.TryParse(departmentIdClaim, out int departmentId))
            {
                return Unauthorized(new { message = "Invalid or missing Kiosk session" });
            }

            var jp = await GetJobProfileQuery(ctx, punchEntryDto.UniqueId, departmentId);

            if (jp == null)
            {
                logger.LogInformation("No Job profile found for the given Id");
                return NotFound(new { message = "No job profile found for the given Id" });
            }

            var jobHistories = await jobHistoryService.GetJobHistoriesByProfileId(jp.Id);

            foreach (var jh in jobHistories)
            {
                if (jh.EndDate == null)
                {
                    break;
                }
                logger.LogInformation("No active job profile found for the id");
                return NotFound(new { message = "No active profile found" });
            }

            PunchEntry? punchEntry = await ctx.PunchEntry.Where(
                    pe => pe.JobProfileId == jp.Id && pe.ClockOut == null
                ).FirstOrDefaultAsync();

            var msg = "";
            bool isClockedOut = false;

            if (punchEntry == null)
            {
                logger.LogInformation("No punch entry were found with clock out null");

                logger.LogInformation("Trying to clock in the employee");

                logger.LogInformation("Checking if they are within clock in buffer time");
                bool clockInTimeOk = jp.EarlyBuffer == null ? IsClockInAllowed(jp.ShiftStartTime, jp.EarlyBufferMin) : IsClockInAllowed(jp.ShiftStartTime, (int)jp.EarlyBuffer);
                if (clockInTimeOk)
                {
                    logger.LogInformation("Clock in was successful");
                    ctx.PunchEntry.Add(new PunchEntry
                    {
                        ClockIn = DateTime.UtcNow,
                        JobProfileId = (int)jp.Id!
                    });
                    msg = "Succefully clocked in!!";
                }
                else
                {
                    logger.LogInformation("Clock in rejected due to trying to clock in too early");
                    return BadRequest(new { message = $"You cannot clock in at this time. Your shift starts on {jp.ShiftStartTime} and you can clock in starting {jp.EarlyBuffer ??jp.EarlyBufferMin} min before" });
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
            .Where(jp => 
                jp.Employee.UniqueId == uniqueId 
                && jp.ProfileTemplate.Unit.DepartmentId == deptId
            )
            .Select(jp => new JobProfileProjection
            {
                Id = jp.Id,
                EarlyBufferMin = jp.ProfileTemplate.EarlyClockInBufferMin,
                ShiftStartTime = jp.ProfileTemplate.ShiftStartTime,
                EarlyBuffer = jp.EarlyBuffer,
            })
            .FirstOrDefault());

        public class JobProfileProjection
        {
            public int Id { get; set; }
            public int EarlyBufferMin { get; set; }
            public TimeOnly ShiftStartTime { get; set; }
            public int? EarlyBuffer { get; set; }
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
