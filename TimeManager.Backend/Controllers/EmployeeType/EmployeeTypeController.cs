using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeManager.Backend.Services;
using TimeManager.Backend.ViewModels;

namespace TimeManager.Backend.Controllers.EmployeeType
{
    [Authorize(Policy = "AdminPolicy")]
    public class EmployeeTypeController : Controller
    {
        private readonly IEmployeeTypeService _employeeTypeService;

        public EmployeeTypeController(IEmployeeTypeService employeeTypeService)
        {
            _employeeTypeService = employeeTypeService;
        }

        public async Task<IActionResult> Index()
        {
            var employeeTypes = await _employeeTypeService.GetEmployeeTypesAsync();
            return View(employeeTypes);
        }

        [HttpGet]
        public async Task<IActionResult> Create() => View(new EmployeeTypeViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EmployeeTypeViewModel evm)
        {
            if (!ModelState.IsValid) return View(evm);
            await _employeeTypeService.CreateEmployeeTypeAsync(new EmployeeTypeDto {
                Name = evm.Name,
                Description = evm.Description,
            });
            TempData["success"] = "Employee Type created";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult >Edit(int id)
        {
            var et = await _employeeTypeService.GetEmployeeTypeByIdAsync(id);
            if (et == null) return NotFound();
            return View(new EmployeeTypeViewModel { Id = id, Name = et.Name, 
            Description  = et.Description });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EmployeeTypeViewModel evm)
        {
            if (!ModelState.IsValid) return View(evm);
            var et = await _employeeTypeService.UpdateEmployeeTypeAsync(id, new EmployeeTypeDto {
                Name = evm.Name,
                Description = evm.Description,
            });
            if (et == null) {
                TempData["error"] = "Employee type not found";
                return View(); }
            TempData["success"] = "Employee Type updated";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _employeeTypeService.DeleteEmployeeTypeByIdAsync(id);
                TempData["success"] = "Employee type deleted";
            } catch (KeyNotFoundException ex)
            {
                TempData["error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
