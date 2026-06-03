using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimeManager.Backend.Controllers.EmployeeManagement.Dto;
using TimeManager.Backend.Data;
using TimeManager.Backend.Models.Employee_Management;

namespace TimeManager.Backend.Controllers.EmployeeManagement
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoleController: ControllerBase
    {
        private readonly HrmsDbContext _context;
        
        public RoleController(HrmsDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Role>>> GetRoles()
        {
            var data = await _context.Role.ToListAsync();
            return data;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Role>> GetRole(int id)
        {
            var role = await _context.Role.FindAsync(id);
            if (role == null)
            {
                return NotFound(new
                {
                    message = "Role not found"
                });
            }

            return role;
        }

        [HttpPost]
        public async Task<ActionResult<Role>> CreateRole([FromBody] RoleDto roleDto)
        {
            var role = _context.Role.Add(new Role { Name = roleDto.Name, Description = roleDto.Description });
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetRole), new { id = role.Entity.Id } , role.Entity);
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<Role>> UpdateRole(int id, [FromBody] RoleDto roleDto)
        {
            var role = await GetRole(id);
            if (role == null)
            {
                return NotFound(new { message = "Role not found" });
            }

            _context.Entry(role).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<Role>> DeleteRole(int id)
        {
            var role = await GetRole(id);
            if (role == null)
            {
                return NotFound(new { message = "Role not found" });
            }

            //_context.Role.Remove(role);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
