using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimeManager.Backend.Controllers.EmployeeManagement.Dto;
using TimeManager.Backend.Data;
using R = TimeManager.Backend.Models.Employee_Management.Role;

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
        public async Task<ActionResult<IEnumerable<R>>> GetRoles()
        {
            var data = await _context.Role.ToListAsync();
            return data;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<R>> GetRole(int id)
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
        public async Task<ActionResult<R>> CreateRole([FromBody] RoleDto roleDto)
        {
            var role = _context.Role.Add(new R { Name = roleDto.Name, Description = roleDto.Description });
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetRole), new { id = role.Entity.Id } , role.Entity);
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<R>> UpdateRole(int id, [FromBody] RoleDto roleDto)
        {
            var role = await GetRoleById(id);
            if (role == null)
            {
                return NotFound(new { message = "Role not found" });
            }

            _context.Entry(role).CurrentValues.SetValues(roleDto);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<R>> DeleteRole(int id)
        {
            var role = await GetRoleById(id);
            if (role == null)
            {
                return NotFound(new { message = "Role not found" });
            }

            _context.Role.Remove(role);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private async Task<R?> GetRoleById(int id)
        {
            R? role = await _context.Role.FindAsync(id);
            return role;
        }
    }
}
