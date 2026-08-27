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
            //if (!ModelState.IsValid) return View(departmentViewModel);
            //await departmentService.CreateDepartmentAsync(departmentViewModel);
            TempData["Success"] = "Department created";
            return View(new DepartmentViewModel());
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
            //if (!ModelState.IsValid) return View(dvm);
            //var d = await departmentService.UpdateDepartmentAsync(id, dvm);

            //if (d == null)
            //{
            //    TempData["error"] = "Error while updating the data";
            //    return View(dvm);
            //}

            TempData["success"] = "Successfully edited the department";
            return View(new DepartmentViewModel
            {
                Id = dvm.Id,
                Name = dvm.Name,
                Description = dvm.Description
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            //try
            //{
            //    await departmentService.DeleteDepartmentByIdAsync(id);
                TempData["success"] = "Successfully removed the department";
            //} catch (KeyNotFoundException ex)
            //{
            //    TempData["error"] = ex.Message;
            //}

            return RedirectToAction(nameof(Index));
        }
    }
}
