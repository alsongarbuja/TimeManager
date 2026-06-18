using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimeManager.Backend.Controllers.EmployeeManagement.Dto;
using TimeManager.Backend.Data;
using PT = TimeManager.Backend.Models.Employee_Management.ProfileTemplate;

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
        public async Task<ActionResult<IEnumerable<PT>>> GetProfileTemplates()
        {
            var data = await _context.ProfileTemplate.Select(pf => new PT
            {
                Id = pf.Id,
                EarlyClockInBufferMin = pf.EarlyClockInBufferMin,
                ShiftStartTime = pf.ShiftStartTime,
                EmployeeTypeId = pf.EmployeeTypeId,
                UnitId = pf.UnitId,
                RoleId = pf.RoleId,
                PayFrequencyId = pf.PayFrequencyId,

                Unit = pf.Unit,
                PayFrequency = pf.PayFrequency,
                Role = pf.Role,
                EmployeeType = pf.EmployeeType,
            }).ToListAsync();
            return data;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PT>> GetProfileTemplate(int id)
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
        public async Task<ActionResult<PT>> CreateProfileTemplate([FromBody] ProfileTemplateDto profileTemplateDto)
        {
            var profileTemplate = _context.ProfileTemplate.Add(new PT { 
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
        public async Task<ActionResult<PT>> UpdateProfileTemplate(int id, [FromBody] ProfileTemplateDto profileTemplateDto)
        {
            var profileTemplate = await GetProfileTemplateById(id);
            if (profileTemplate == null)
            {
                return NotFound(new { message = "ProfileTemplate not found" });
            }

            _context.Entry(profileTemplate).CurrentValues.SetValues(profileTemplateDto);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<PT>> DeleteProfileTemplate(int id)
        {
            var profileTemplate = await GetProfileTemplateById(id);
            if (profileTemplate == null)
            {
                return NotFound(new { message = "ProfileTemplate not found" });
            }

            _context.ProfileTemplate.Remove(profileTemplate);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private async Task<PT?> GetProfileTemplateById(int id)
        {
            PT? pt = await _context.ProfileTemplate.FindAsync(id);
            return pt;
        }
    }
}
