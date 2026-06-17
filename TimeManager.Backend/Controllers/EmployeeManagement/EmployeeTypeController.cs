using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimeManager.Backend.Controllers.EmployeeManagement.Dto;
using TimeManager.Backend.Data;
using ET = TimeManager.Backend.Models.Employee_Management.EmployeeType;

namespace TimeManager.Backend.Controllers.EmployeeManagement
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeTypeController : ControllerBase
    {
        private readonly HrmsDbContext _context;

        public EmployeeTypeController(HrmsDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ET>>> GetEmployeeTypes()
        {
            var data = await _context.EmployeeType.ToListAsync();
            return data;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ET>> GetEmployeeType(int id)
        {
            var employeeType = await _context.EmployeeType.FindAsync(id);
            if (employeeType == null)
            {
                return NotFound(new
                {
                    message = "EmployeeType not found"
                });
            }

            return employeeType;
        }

        [HttpPost]
        public async Task<ActionResult<ET>> CreateEmployeeType([FromBody] EmployeeTypeDto employeeTypeDto)
        {
            var employeeType = _context.EmployeeType.Add(new ET { Name = employeeTypeDto.Name, Description = employeeTypeDto.Description });
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEmployeeType), new { id = employeeType.Entity.Id }, employeeType.Entity);
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<ET>> UpdateEmployeeType(int id, [FromBody] EmployeeTypeDto employeeTypeDto)
        {
            var employeeType = await GetEmployeeTypeById(id);
            if (employeeType == null)
            {
                return NotFound(new { message = "EmployeeType not found" });
            }

            _context.Entry(employeeType).CurrentValues.SetValues(employeeTypeDto);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ET>> DeleteEmployeeType(int id)
        {
            var employeeType = await GetEmployeeTypeById(id);
            if (employeeType == null)
            {
                return NotFound(new { message = "EmployeeType not found" });
            }

            _context.EmployeeType.Remove(employeeType);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private async Task<ET?> GetEmployeeTypeById(int id)
        {
            ET? et = await _context.EmployeeType.FindAsync(id);
            return et;
        }
    }
}
