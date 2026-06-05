using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimeManager.Backend.Controllers.EmployeeManagement.Dto;
using TimeManager.Backend.Data;
using TimeManager.Backend.Models.Employee_Management;

namespace TimeManager.Backend.Controllers.EmployeeManagement
{
    [ApiController]
    [Route("api/[controller]")]
    public class PayFrequencyController : ControllerBase
    {
        private readonly HrmsDbContext _context;

        public PayFrequencyController(HrmsDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PayFrequency>>> GetPayFrequencys()
        {
            var data = await _context.PayFrequency.ToListAsync();
            return data;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PayFrequency>> GetPayFrequency(int id)
        {
            var payFrequency = await _context.PayFrequency.FindAsync(id);
            if (payFrequency == null)
            {
                return NotFound(new
                {
                    message = "PayFrequency not found"
                });
            }

            return payFrequency;
        }

        [HttpPost]
        public async Task<ActionResult<PayFrequency>> CreatePayFrequency([FromBody] PayFrequencyDto payFrequencyDto)
        {
            var payFrequency = _context.PayFrequency.Add(new PayFrequency { Name = payFrequencyDto.Name, Description = payFrequencyDto.Description });
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPayFrequency), new { id = payFrequency.Entity.Id }, payFrequency.Entity);
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<PayFrequency>> UpdatePayFrequency(int id, [FromBody] PayFrequencyDto payFrequencyDto)
        {
            var payFrequency = await GetPayFrequencyById(id);
            if (payFrequency == null)
            {
                return NotFound(new { message = "PayFrequency not found" });
            }

            _context.Entry(payFrequency).CurrentValues.SetValues(payFrequencyDto);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<PayFrequency>> DeletePayFrequency(int id)
        {
            var payFrequency = await GetPayFrequencyById(id);
            if (payFrequency == null)
            {
                return NotFound(new { message = "PayFrequency not found" });
            }

            _context.PayFrequency.Remove(payFrequency);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private async Task<PayFrequency?> GetPayFrequencyById(int id)
        {
            PayFrequency? pf = await _context.PayFrequency.FindAsync(id);
            return pf;
        }
    }
}
