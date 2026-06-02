using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using TimeManager.Backend.Controllers.Organization.Dto;
using TimeManager.Backend.Data;
using TimeManager.Backend.Models.Organization_Management;

namespace TimeManager.Backend.Controllers.Organization
{
    [ApiController]
    [Route("api/[controller]")]
    public class DepartmentController: ControllerBase
    {
        private readonly HrmsDbContext _context;

        public DepartmentController(HrmsDbContext context) {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Department>>> GetDepartments()
        {
            var data = await _context.Department.ToListAsync();
            return data;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Department>> GetDepartment(int id)
        {
           var department = await _context.Department.FindAsync(id);

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
        public async Task<ActionResult<Department>> CreateDepartment([FromBody] DepartmentDto departmentDto)
        {
            Debug.WriteLine("Hello in the server");
            Debug.WriteLine("Test");
            Debug.WriteLine(departmentDto.Name);

            var data = _context.Department.Add(new Department { Name = departmentDto.Name, Description = departmentDto.Description });

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDepartment), new { id = data.Entity.Id }, data.Entity);
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<Department>> UpdateDepartment(int id, [FromBody] DepartmentDto departmentDto) { 
            var department = await GetDepartment(id);

            if (department == null)
            {
                return NotFound(new { message = $"Product with ID {id} does not exist." });
            }

            _context.Entry(departmentDto).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            return NoContent();
        }


        //[HttpDelete("{id}")]
        //public async Task<ActionResult> DeleteDepartment(int id) { 
        //    var department = await GetDepartment(id);

        //    if (department == null)
        //    {
        //        return NotFound(new { message = $"Product with ID {id} does not exist." });
        //    }

        //    _context.Department.Remove(department);
        //    await _context.SaveChangesAsync();

        //    return NoContent();
        //}
    }
}
