using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeManager.Backend.Extensions;
using TimeManager.Backend.Services;
using TimeManager.Backend.Utility;
using TimeManager.Backend.ViewModels;
using U = TimeManager.Backend.Models.Organization_Management.Unit;

namespace TimeManager.Backend.Controllers.Unit
{
    [Authorize(Policy = "AdminPolicy")]
    public class UnitController(
        IUnitService unitService, 
        IDepartmentService departmentService,
        ILogger<U> logger
    ) : Controller
    {
        public async Task<IActionResult> Index()
        {
            int? departmentId = HttpContext.Session.GetDepartmentId();
            var units = await unitService.GetUnitsAysnc(departmentId);
            return View(units);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var departments = await departmentService.GetDepartmentOptionsAsync();
            return View(new UnitViewModel
            {
                Departments = departments,
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UnitViewModel uvm)
        {
            if (!ModelState.IsValid)
            {
                ModelValidationLog.LogModelStateValidationFailedLogs(logger, ModelState);
                return View(uvm);
            }
            int? departmentId = HttpContext.Session.GetDepartmentId();

            await unitService.CreateUnitAsync(uvm);
            TempData["Success"] = "Unit created";
            var departments = departmentId == null ? await departmentService.GetDepartmentOptionsAsync() : [];

            return View(new UnitViewModel
            {
                Departments = departments
            });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var unit = await unitService.GetUnitByIdAsync(id);

            if (unit == null) return NotFound();
            return View(unit);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UnitViewModel uvm)
        {
            if (!ModelState.IsValid) return View(uvm);
            int? departmentId = HttpContext.Session.GetDepartmentId();
            var d = await unitService.UpdateUnitAsync(id, uvm);

            if (d == null)
            {
                TempData["error"] = "Error while updating the data";
                return View(uvm);
            }

            TempData["success"] = "Successfully edited the unit";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await unitService.DeleteUnitByIdAsync(id);
                TempData["success"] = "Successfully removed the unit";
            } catch(KeyNotFoundException ex)
            {
                TempData["error"] = ex.Message;
            }
            
            return RedirectToAction(nameof(Index));
        }
    }
}
