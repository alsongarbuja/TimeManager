using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeManager.Backend.Extensions;
using TimeManager.Backend.Models.Requests;
using TimeManager.Backend.Models.Responses;
using TimeManager.Backend.Services;
using TimeManager.Backend.ViewModels;

namespace TimeManager.Backend.Controllers.Employee
{
    [Authorize(Policy = "AdminPolicy")]
    public class EmployeeController(IEmployeeService employeeService, IDepartmentService departmentService, IUserService userService) : Controller
    {
        public async Task<IActionResult> Index([FromQuery] PaginationFilter filter)
        {
            int? departmentId = HttpContext.Session.GetDepartmentId();
            PagedResponse<EmployeeViewModel> employees = await employeeService.GetEmployeesAsync(departmentId, filter);
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
            try
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
            } catch (ArgumentException ex)
            {
                ModelState.AddModelError("EmployeeView.UniqueId", ex.Message);

                return View(new EmployeeData
                {
                    EmployeeView = employeeData.EmployeeView,
                    Departments = (await departmentService.GetDepartmentOptionsAsync(employeeData.UserId)),
                    Users = (await userService.GetUserOptionsAsync(employeeData.UserId))
                });
            }
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
