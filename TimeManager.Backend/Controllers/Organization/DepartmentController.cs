using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimeManager.Backend.Controllers.Organization.Dto;
using TimeManager.Backend.Data;
using D = TimeManager.Backend.Models.Organization_Management.Department;

namespace TimeManager.Backend.Controllers.Organization
{
    [ApiController]
    [Route("api/[controller]")]
    public class DepartmentController: ControllerBase
    {
        private readonly HrmsDbContext _context;
        private readonly ILogger<DepartmentController> _logger;

        public DepartmentController(HrmsDbContext context, ILogger<DepartmentController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<D>>> GetDepartments()
        {
            var data = await _context.Department.ToListAsync();
            return data;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<D>> GetDepartment(int id)
        {
           var department = await GetDepartmentById(id);

            if (department == null)
            {
                return NotFound(new
                {
                    message = "Department not found"
                });
            }

            return department;
        }


        [HttpPost]
        public async Task<ActionResult<D>> CreateDepartment([FromBody] DepartmentDto departmentDto)
        {
            var data = _context.Department.Add(new D{ Name = departmentDto.Name, Description = departmentDto.Description });

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDepartment), new { id = data.Entity.Id }, data.Entity);
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<D>> UpdateDepartment(int id, [FromBody] DepartmentDto departmentDto) { 
            var department = await GetDepartmentById(id);

            if (department == null)
            {
                return NotFound(new { message = $"Department with ID {id} does not exist." });
            }

            _context.Entry(department).CurrentValues.SetValues(departmentDto);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteDepartment(int id)
        {
            var department = await GetDepartmentById(id);

            if (department == null)
            {
                return NotFound(new { message = $"Department with ID {id} does not exist." });
            }

            _context.Department.Remove(department);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private async Task<D?> GetDepartmentById(int id)
        {
            D? d = await _context.Department.FindAsync(id);
            return d;
        }
    }
}
