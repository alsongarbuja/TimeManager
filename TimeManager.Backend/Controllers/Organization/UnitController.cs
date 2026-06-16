using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimeManager.Backend.Controllers.Organization.Dto;
using TimeManager.Backend.Data;
using U = TimeManager.Backend.Models.Organization_Management.Unit;

namespace TimeManager.Backend.Controllers.Organization
{
    [ApiController]
    [Route("api/[controller]")]
    public class UnitController: ControllerBase
    {
        private readonly HrmsDbContext _context;

        public UnitController(HrmsDbContext context)
        {
            this._context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<U>>> GetUnits()
        {
            var data = await _context.Unit.Select(u => new U
            {
                Id = u.Id,
                Name = u.Name,
                Description = u.Description,
                Index = u.Index,
                Department = u.Department,
            }).ToListAsync();
            return data;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<U>> GetUnit(int id)
        {
            var unit = await _context.Unit.FindAsync(id);

            if (unit == null)
            {
                return NotFound(new { message = "Unit not found" });
            }

            return unit;
        }

        [HttpPost]
        public async Task<ActionResult<U>> CreateUnit([FromBody] UnitDto unitDto)
        {
            var data = _context.Unit.Add(new U
            {
                Name = unitDto.Name,
                Description = unitDto.Description,
                Index = unitDto.Index,
                DepartmentId = unitDto.DepartmentId,
            });
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUnit), new { id = data.Entity.Id }, data.Entity);
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<U>> UpdateUnit(int id, [FromBody] UnitDto unitDto)
        {
            var unit = await GetUnitById(id);

            if (unit == null)
            {
                return NotFound(new { message = "Unit not found" });
            }

            _context.Entry(unit).CurrentValues.SetValues(unitDto);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteUnit(int id)
        {
            var unit = await GetUnitById(id);

            if (unit == null)
            {
                return NotFound(new { message = "Unit not found" });
            }

            _context.Unit.Remove(unit);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private async Task<U?> GetUnitById(int id)
        {
            U? u = await _context.Unit.FindAsync(id);
            return u;
        }
    }
}
