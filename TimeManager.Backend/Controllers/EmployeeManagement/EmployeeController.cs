using Microsoft.AspNetCore.Mvc;
using TimeManager.Backend.Controllers.EmployeeManagement.Dto;
using Emp = TimeManager.Backend.Models.Employee_Management.Employee;
using TimeManager.Backend.Services;
using TimeManager.Backend.ViewModels;

namespace TimeManager.Backend.Controllers.EmployeeManagement
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;

        public EmployeeController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Emp>>> GetEmployees()
        {
            IEnumerable<EmployeeViewModel> data = await _employeeService.GetEmployeesAsync();

            List<Emp> employees = new List<Emp>();

            foreach (var d in data) {
                employees.Add(new Emp {
                    Id = d.Id,
                    UniqueId = d.UniqueId,
                    FirstName = d.FirstName,
                    LastName = d.LastName,
                });
            }

            return employees;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Emp>> GetEmployee(int id)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(id);
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
        public async Task<ActionResult> CreateEmployee([FromBody] EmployeeDto employeeDto)
        {
            await _employeeService.CreateEmployeeAsync(employeeDto);
            return Ok();
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<Emp>> UpdateEmployee(int id, [FromBody] EmployeeDto employeeDto)
        {
            var e = await _employeeService.UpdateEmployeeAsync(id, employeeDto);
            if (e == null) return NotFound();
            return e;
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<int>> DeleteEmployee(int id)
        {
            var i = await _employeeService.DeleteEmployeeByIdAsync(id);
            if (i == null) return NotFound();
            return i;
        }
    }
}
