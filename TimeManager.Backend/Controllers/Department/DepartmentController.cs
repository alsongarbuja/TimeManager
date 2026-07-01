using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeManager.Backend.Services;
using TimeManager.Backend.ViewModels;

namespace TimeManager.Backend.Controllers.Department
{
    [Authorize(Policy = "AdminPolicy")]
    public class DepartmentController(IDepartmentService departmentService) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var departments = await departmentService.GetDepartmentsAsync();
            return View(departments);
        }

        [HttpGet]
        public async Task<IActionResult> Create() => View(new DepartmentViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DepartmentViewModel departmentViewModel)
        {
            if (!ModelState.IsValid) return View(departmentViewModel);
            await departmentService.CreateDepartmentAsync(new Services.DepartmentDto
            {
                Name = departmentViewModel.Name,
                Description = departmentViewModel.Description,
            });
            TempData["Success"] = "Department created";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var department = await departmentService.GetDepartmentByIdAsync(id);
            if (department == null) return NotFound();
            return View(new DepartmentViewModel
            {
                Id = department.Id,
                Name = department.Name,
                Description = department.Description
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DepartmentViewModel dvm)
        {
            if (!ModelState.IsValid) return View(dvm);
            var d = await departmentService.UpdateDepartmentAsync(id, new DepartmentDto { Name = dvm.Name,
                Description = dvm.Description
            });

            if (d == null)
            {
                TempData["error"] = "Error while updating the data";
                return View(dvm);
            }

            TempData["success"] = "Successfully edited the data";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await departmentService.DeleteDepartmentByIdAsync(id);
                TempData["success"] = "Successfully removed the data";
            } catch (KeyNotFoundException ex)
            {
                TempData["error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
