using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimeManager.Backend.Controllers.Device.Dto;
using TimeManager.Backend.Data;
using K = TimeManager.Backend.Models.Device_Management.Kiosk;

namespace TimeManager.Backend.Controllers.Device
{
    [ApiController]
    [Route("api/[controller]")]
    public class KioskController : ControllerBase
    {
        private readonly HrmsDbContext _context;

        public KioskController(HrmsDbContext context)
        {
            this._context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<K>>> GetKiosks()
        {
            var data = await _context.Kiosk.ToListAsync();
            return data;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<K>> GetKiosk(int id)
        {
            var kiosk = await _context.Kiosk.FindAsync(id);

            if (kiosk == null)
            {
                return NotFound(new { message = "Kiosk not found" });
            }

            return kiosk;
        }

        [HttpPost]
        public async Task<ActionResult<K>> CreateKiosk([FromBody] KioskDto kioskDto)
        {
            var data = _context.Kiosk.Add(new K
            {
                Name = kioskDto.Name,
                Description = kioskDto.Description,
                DepartmentId = kioskDto.DepartmentId,
                AllowedIPAddress = kioskDto.AllowedIPAddress,
            });
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetKiosk), new { id = data.Entity.Id }, data.Entity);
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<K>> UpdateKiosk(int id, [FromBody] KioskDto kioskDto)
        {
            var kiosk = await GetKiosk(id);

            if (kiosk == null)
            {
                return NotFound(new { message = "Kiosk not found" });
            }

            _context.Entry(kiosk).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteKiosk(int id)
        {
            var kiosk = await GetKioskById(id);
            
            if (kiosk == null)
            {
                return NotFound(new { message = "Kiosk not found" });
            }

            _context.Kiosk.Remove(kiosk);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private async Task<K?> GetKioskById(int id)
        {
            K? k = await _context.Kiosk.FindAsync(id);
            return k;
        }
    }
}
