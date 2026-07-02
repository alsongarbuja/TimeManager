using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeManager.Backend.Extensions;
using TimeManager.Backend.Services;
using TimeManager.Backend.ViewModels;

namespace TimeManager.Backend.Controllers.Employee
{
    [Authorize(Policy = "AdminPolicy")]
    public class EmployeeController(IEmployeeService employeeService, IDepartmentService departmentService, IUserService userService) : Controller
    {
        public async Task<IActionResult> Index()
        {
            int? departmentId = HttpContext.Session.GetDepartmentId();
            var employees = await employeeService.GetEmployeesAsync(departmentId);
            return View(employees);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            EmployeeData employeeData = new() { 
                EmployeeView = new EmployeeViewModel(),
                Departments = (await departmentService.GetDepartmentOptionsAsync()),
                Users = (await userService.GetUserOptionsAsync())
            };
            return View(employeeData);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EmployeeData employeeData)
        {
            int? departmentId = HttpContext.Session.GetDepartmentId();
            int id = await employeeService.CreateEmployeeAsync(new EmployeeDto
            {
                Email = employeeData.EmployeeView.Email,
                UniqueId = employeeData.EmployeeView.UniqueId,
                FirstName = employeeData.EmployeeView.FirstName,
                LastName = employeeData.EmployeeView.LastName,
                UserId = employeeData.UserId,
                DepartmentId = departmentId ?? employeeData.DepartmentId,
            });
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var e = await employeeService.GetEmployeeByIdAsync(id);
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
                Users = (await userService.GetUserOptionsAsync(e.UserId)),
                Departments = (await departmentService.GetDepartmentOptionsAsync(departmentId ?? 0))
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EmployeeData employeeData)
        {
            var e = await employeeService.UpdateEmployeeAsync(id, new EmployeeDto
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
                await employeeService.DeleteEmployeeByIdAsync(id);
                TempData["success"] = "Employee deleted";
            } catch (KeyNotFoundException ex)
            {
                TempData["error"] = ex.Message;
            }
            
            return RedirectToAction(nameof(Index));
        }

    }    
}
