using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimeManager.Backend.Controllers.EmployeeManagement.Dto;
using TimeManager.Backend.Data;
using TimeManager.Backend.Models.Employee_Management;

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
        public async Task<ActionResult<IEnumerable<EmployeeType>>> GetEmployeeTypes()
        {
            var data = await _context.EmployeeType.ToListAsync();
            return data;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EmployeeType>> GetEmployeeType(int id)
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
        public async Task<ActionResult<EmployeeType>> CreateEmployeeType([FromBody] EmployeeTypeDto employeeTypeDto)
        {
            var employeeType = _context.EmployeeType.Add(new EmployeeType { Name = employeeTypeDto.Name, Description = employeeTypeDto.Description });
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEmployeeType), new { id = employeeType.Entity.Id }, employeeType.Entity);
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<EmployeeType>> UpdateEmployeeType(int id, [FromBody] EmployeeTypeDto employeeTypeDto)
        {
            var employeeType = await GetEmployeeType(id);
            if (employeeType == null)
            {
                return NotFound(new { message = "EmployeeType not found" });
            }

            _context.Entry(employeeType).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<EmployeeType>> DeleteEmployeeType(int id)
        {
            var employeeType = await GetEmployeeType(id);
            if (employeeType == null)
            {
                return NotFound(new { message = "EmployeeType not found" });
            }

            //_context.EmployeeType.Remove(employeeType);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
