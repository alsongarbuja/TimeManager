using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimeManager.Backend.Controllers.EmployeeManagement.Dto;
using TimeManager.Backend.Data;
using TimeManager.Backend.Models.Employee_Management;

namespace TimeManager.Backend.Controllers.EmployeeManagement
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeController : ControllerBase
    {
        private readonly HrmsDbContext _context;

        public EmployeeController(HrmsDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Employee>>> GetEmployees()
        {
            var data = await _context.Employee.ToListAsync();
            return data;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Employee>> GetEmployee(int id)
        {
            var employee = await _context.Employee.FindAsync(id);
            if (employee == null)
            {
                return NotFound(new
                {
                    message = "Employee not found"
                });
            }

            return employee;
        }

        [HttpPost]
        public async Task<ActionResult<Employee>> CreateEmployee([FromBody] EmployeeDto employeeDto)
        {
            var employee = _context.Employee.Add(new Employee
            {
                Email = employeeDto.Email,
                FirstName = employeeDto.FirstName,
                LastName = employeeDto.LastName,
                UniqueId = employeeDto.UniqueId,
            });
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEmployee), new { id = employee.Entity.Id }, employee.Entity);
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<Employee>> UpdateEmployee(int id, [FromBody] EmployeeDto employeeDto)
        {
            var employee = await GetEmployeeById(id);
            if (employee == null)
            {
                return NotFound(new { message = "Employee not found" });
            }

            _context.Entry(employee).CurrentValues.SetValues(employeeDto);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<Employee>> DeleteEmployee(int id)
        {
            var employee = await GetEmployee(id);
            if (employee == null)
            {
                return NotFound(new { message = "Employee not found" });
            }

            //_context.Employee.Remove(employee);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private async Task<Employee?> GetEmployeeById(int id)
        {
            Employee? e = await _context.Employee.FindAsync(id);
            return e;
        }
    }
}
