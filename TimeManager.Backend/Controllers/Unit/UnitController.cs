using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeManager.Backend.Controllers.Organization.Dto;
using TimeManager.Backend.Services;
using TimeManager.Backend.ViewModels;

namespace TimeManager.Backend.Controllers.Unit
{
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class UnitController : Controller
    {
        private readonly IUnitService unitService;
        private readonly IDepartmentService departmentService;

        public UnitController(IUnitService unitService, IDepartmentService departmentService)
        {
            this.unitService = unitService;
            this.departmentService = departmentService;
        }

        public async Task<IActionResult> Index()
        {
            var units = await this.unitService.GetUnitsAysnc();
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
        public async Task<IActionResult> Create(UnitViewModel unitViewModel)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Where(x => x.Value.Errors.Count > 0)
                           .Select(x => new
                           {
                               Key = x.Key,
                               Errors = string.Join(", ", x.Value.Errors.Select(e => e.ErrorMessage))
                           });

                foreach (var error in errors)
                {
                    Console.WriteLine($"Field: {error.Key} | Error: {error.Errors}");
                }
                return View(unitViewModel);
            }
            await this.unitService.CreateUnitAsync(new UnitDto
            {
                Name = unitViewModel.Name,
                Description = unitViewModel.Description,
                DepartmentId = unitViewModel.DepartmentId,
                Index = unitViewModel.Index,
            });
            TempData["Success"] = "Unit created";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var unit = await this.unitService.GetUnitByIdAsync(id);
            if (unit == null) return NotFound();
            return View(new UnitViewModel
            {
                Id = unit.Id,
                Name = unit.Name,
                Index = unit.Index,
                Description = unit.Description,
                DepartmentName = unit.Department.Name,
                DepartmentId = unit.DepartmentId,
                Departments = (await departmentService.GetDepartmentOptionsAsync())
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UnitViewModel uvm)
        {
            if (!ModelState.IsValid) return View(uvm);
            var d = await this.unitService.UpdateUnitAsync(id, new UnitDto
            {
                Name = uvm.Name,
                Index = uvm.Index,
                Description = uvm.Description,
                DepartmentId = uvm.DepartmentId,
            });

            if (d == null)
            {
                TempData["error"] = "Error while updating the data";
                return View(uvm);
            }

            TempData["success"] = "Successfully edited the data";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var data = await this.unitService.DeleteUnitByIdAsync(id);
            if (data == null)
            {
                TempData["error"] = "Error while deleting the data";
            }
            else
            {
                TempData["success"] = "Successfully removed the data";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
