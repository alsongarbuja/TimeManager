using Microsoft.AspNetCore.Mvc;
using System.Xml;
using TimeManager.Backend.Controllers.EmployeeManagement.Dto;
using TimeManager.Backend.Services;
using TimeManager.Backend.ViewModels;

namespace TimeManager.Backend.Controllers.Employee
{
    public class EmployeeController : Controller
    {
        private readonly IEmployeeService _employeeService;

        public EmployeeController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        public async Task<IActionResult> Index()
        {
            var employees = await _employeeService.GetEmployeesAsync();
            return View(employees);
        }

        [HttpGet]
        public async Task<IActionResult> Create() => View(new EmployeeViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EmployeeViewModel employeeViewModel)
        {
            await _employeeService.CreateEmployeeAsync(new EmployeeDto
            {
                Email = employeeViewModel.Email,
                UniqueId = employeeViewModel.UniqueId,
                FirstName = employeeViewModel.FirstName,
                LastName = employeeViewModel.LastName,
            });
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var e = await _employeeService.GetEmployeeByIdAsync(id);
            if (e == null) return NotFound();
            TempData["success"] = "Employee created";
            return View(new EmployeeViewModel
            {
                Id = e.Id,
                Email = e.Email,
                UniqueId = e.UniqueId,
                FirstName = e.FirstName, 
                LastName = e.LastName,
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EmployeeViewModel employeeViewModel)
        {
            var e = await _employeeService.UpdateEmployeeAsync(id, new EmployeeDto
            {
                Email = employeeViewModel.Email,
                UniqueId = employeeViewModel.UniqueId,
                FirstName = employeeViewModel.FirstName,
                LastName = employeeViewModel.LastName,
            });
            if (e == null)
            {
                TempData["error"] = "Employee not found";
                return View(employeeViewModel);
            }
            TempData["success"] = "Employee updated";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var e = await _employeeService.DeleteEmployeeByIdAsync(id);
            if (e == null)
            {
                TempData["error"] = "Employee not found";
            } else
            {
                TempData["success"] = "Employee deleted";
            }
            return RedirectToAction(nameof(Index));
        }

    }    
}
