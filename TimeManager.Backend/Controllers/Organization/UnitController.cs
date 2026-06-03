using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimeManager.Backend.Controllers.Organization.Dto;
using TimeManager.Backend.Data;
using TimeManager.Backend.Models.Organization_Management;

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
        public async Task<ActionResult<IEnumerable<Unit>>> GetUnits()
        {
            var data = await _context.Unit.Select(u => new Unit
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
        public async Task<ActionResult<Unit>> GetUnit(int id)
        {
            var unit = await _context.Unit.FindAsync(id);

            if (unit == null)
            {
                return NotFound(new { message = "Unit not found" });
            }

            return unit;
        }

        [HttpPost]
        public async Task<ActionResult<Unit>> CreateUnit([FromBody] UnitDto unitDto)
        {
            var data = _context.Unit.Add(new Unit
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
        public async Task<ActionResult<Unit>> UpdateUnit(int id, [FromBody] UnitDto unitDto)
        {
            var unit = await GetUnit(id);

            if (unit == null)
            {
                return NotFound(new { message = "Unit not found" });
            }

            _context.Entry(unit).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        //[HttpDelete("{id}")]
        //public async void DeleteUnit(int id)
        //{
        //    var unit = await GetUnit(id);
        //    Unit? value = unit.Value;

        //    if (value == null)
        //    {
        //        NotFound(new { message = "Unit not found" });
        //        return;
        //    }

        //    await _context.Unit.Remove();
        //}
    }
}
