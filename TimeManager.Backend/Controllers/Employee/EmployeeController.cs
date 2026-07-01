using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeManager.Backend.Extensions;
using TimeManager.Backend.Services;
using TimeManager.Backend.ViewModels;

namespace TimeManager.Backend.Controllers.Employee
{
    [Authorize(Policy = "AdminPolicy")]
    public class EmployeeController : Controller
    {
        private readonly IEmployeeService _employeeService;
        private readonly IDepartmentService _departmentService;
        private readonly IUserService _userService;
        //private readonly IEmployeeDepartmentService _employeeDepartmentService;

        public EmployeeController(IEmployeeService employeeService, IDepartmentService departmentService, IUserService userService)
        {
            _employeeService = employeeService;
            _departmentService = departmentService;
            _userService = userService;
            //_employeeDepartmentService = employeeDepartmentService;
        }

        public async Task<IActionResult> Index()
        {
            int? departmentId = HttpContext.Session.GetDepartmentId();
            var employees = await _employeeService.GetEmployeesAsync(departmentId);
            return View(employees);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            EmployeeData employeeData = new EmployeeData { 
                EmployeeView = new EmployeeViewModel(),
                Departments = (await _departmentService.GetDepartmentOptionsAsync()),
                Users = (await _userService.GetUserOptionsAsync())
            };
            return View(employeeData);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EmployeeData employeeData)
        {
            int id = await _employeeService.CreateEmployeeAsync(new EmployeeDto
            {
                Email = employeeData.EmployeeView.Email,
                UniqueId = employeeData.EmployeeView.UniqueId,
                FirstName = employeeData.EmployeeView.FirstName,
                LastName = employeeData.EmployeeView.LastName,
                UserId = employeeData.UserId,
                DepartmentId = employeeData.DepartmentId,
            });
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var e = await _employeeService.GetEmployeeByIdAsync(id);
            if (e == null) return NotFound();
            int? departmentId = HttpContext.Session.GetDepartmentId();
            return View(new EmployeeData
            {
                EmployeeView = new EmployeeViewModel
                {
                    Id = e.Id,
                    UniqueId = e.UniqueId,
                    FirstName = e.FirstName, 
                    LastName = e.LastName,
                },
                Users = (await _userService.GetUserOptionsAsync(e.UserId)),
                Departments = (await _departmentService.GetDepartmentOptionsAsync(departmentId ?? 0))
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EmployeeData employeeData)
        {
            var e = await _employeeService.UpdateEmployeeAsync(id, new EmployeeDto
            {
                Email = employeeData.EmployeeView.Email,
                UniqueId = employeeData.EmployeeView.UniqueId,
                FirstName = employeeData.EmployeeView.FirstName,
                LastName = employeeData.EmployeeView.LastName,
                UserId = employeeData.UserId,
                DepartmentId = employeeData.DepartmentId,
            });
            if (e == null)
            {
                TempData["error"] = "Employee not found";
                return View(employeeData);
            }

            //await _employeeDepartmentService.UpdateEmployeeDepartmentAsync(e.Id, )

            TempData["success"] = "Employee updated";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _employeeService.DeleteEmployeeByIdAsync(id);
                TempData["success"] = "Employee deleted";
            } catch (KeyNotFoundException ex)
            {
                TempData["error"] = ex.Message;
            }
            
            return RedirectToAction(nameof(Index));
        }

    }    
}
