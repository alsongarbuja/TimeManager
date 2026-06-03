using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimeManager.Backend.Controllers.EmployeeManagement.Dto;
using TimeManager.Backend.Data;
using TimeManager.Backend.Models.Employee_Management;

namespace TimeManager.Backend.Controllers.EmployeeManagement
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProfileTemplateController : ControllerBase
    {
        private readonly HrmsDbContext _context;

        public ProfileTemplateController(HrmsDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProfileTemplate>>> GetProfileTemplates()
        {
            var data = await _context.ProfileTemplate.ToListAsync();
            return data;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProfileTemplate>> GetProfileTemplate(int id)
        {
            var profileTemplate = await _context.ProfileTemplate.FindAsync(id);
            if (profileTemplate == null)
            {
                return NotFound(new
                {
                    message = "ProfileTemplate not found"
                });
            }

            return profileTemplate;
        }

        [HttpPost]
        public async Task<ActionResult<ProfileTemplate>> CreateProfileTemplate([FromBody] ProfileTemplateDto profileTemplateDto)
        {
            var profileTemplate = _context.ProfileTemplate.Add(new ProfileTemplate { 
                UnitId = profileTemplateDto.UnitId,
                RoleId = profileTemplateDto.RoleId,
                PayFrequencyId = profileTemplateDto.PayFrequencyId,
                EmployeeTypeId = profileTemplateDto.EmployeeTypeId,
                ShiftStartTime = profileTemplateDto.ShiftStartTime,
                EarlyClockInBufferMin = profileTemplateDto.EarlyClockInBufferMin,
            });
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProfileTemplate), new { id = profileTemplate.Entity.Id }, profileTemplate.Entity);
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<ProfileTemplate>> UpdateProfileTemplate(int id, [FromBody] ProfileTemplateDto profileTemplateDto)
        {
            var profileTemplate = await GetProfileTemplate(id);
            if (profileTemplate == null)
            {
                return NotFound(new { message = "ProfileTemplate not found" });
            }

            _context.Entry(profileTemplate).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ProfileTemplate>> DeleteProfileTemplate(int id)
        {
            var profileTemplate = await GetProfileTemplate(id);
            if (profileTemplate == null)
            {
                return NotFound(new { message = "ProfileTemplate not found" });
            }

            //_context.ProfileTemplate.Remove(profileTemplate);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
