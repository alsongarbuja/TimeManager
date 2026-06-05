using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimeManager.Backend.Controllers.EmployeeManagement.Dto;
using TimeManager.Backend.Data;
using TimeManager.Backend.Models.Employee_Management;

namespace TimeManager.Backend.Controllers.EmployeeManagement
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobProfileController : ControllerBase
    {
        private readonly HrmsDbContext _context;

        public JobProfileController(HrmsDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<JobProfileDto>>> GetJobProfiles()
        {
            var data = await _context.JobProfile.Select(jp => new JobProfileDto { 
                Id = jp.Id,
                EmployeeId = jp.EmployeeId,
                ProfileTemplateId = jp.ProfileTemplateId,

                EmployeeString = $"{jp.Employee.FirstName} {jp.Employee.LastName}",
                ProfileTemplateString = $"{jp.ProfileTemplate.Unit.Name} / {jp.ProfileTemplate.Role.Name}",
            }).ToListAsync();
            return data;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<JobProfile>> GetJobProfile(int id)
        {
            var jobProfile = await _context.JobProfile.FindAsync(id);
            if (jobProfile == null)
            {
                return NotFound(new
                {
                    message = "JobProfile not found"
                });
            }

            return jobProfile;
        }

        [HttpPost]
        public async Task<ActionResult<JobProfile>> CreateJobProfile([FromBody] JobProfileDto jobProfileDto)
        {
            var jobProfile = _context.JobProfile.Add(new JobProfile { 
                EmployeeId = jobProfileDto.EmployeeId,
                ProfileTemplateId = jobProfileDto.ProfileTemplateId,
            });
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetJobProfile), new { id = jobProfile.Entity.Id }, jobProfile.Entity);
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<JobProfile>> UpdateJobProfile(int id, [FromBody] JobProfileDto jobProfileDto)
        {
            var jobProfile = await GetJobProfileById(id);
            if (jobProfile == null)
            {
                return NotFound(new { message = "JobProfile not found" });
            }

            _context.Entry(jobProfile).CurrentValues.SetValues(jobProfileDto);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<JobProfile>> DeleteJobProfile(int id)
        {
            var jobProfile = await GetJobProfileById(id);
            if (jobProfile == null)
            {
                return NotFound(new { message = "JobProfile not found" });
            }

            _context.JobProfile.Remove(jobProfile);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private async Task<JobProfile?> GetJobProfileById(int id)
        {
            JobProfile? jp = await _context.JobProfile.FindAsync(id);
            return jp;
        }
    }
}
