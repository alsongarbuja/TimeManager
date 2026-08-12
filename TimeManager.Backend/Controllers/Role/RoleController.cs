using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeManager.Backend.Common;
using TimeManager.Backend.Services;
using TimeManager.Backend.ViewModels;

namespace TimeManager.Backend.Controllers.Role
{
    [Authorize(Roles = AppConstants.SUPER_ADMIN_ROLE)]
    public class RoleController(IRoleService roleService) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var roles = await roleService.GetRolesAsync();
            return View(roles);
        }

        [HttpGet]
        public async Task<IActionResult> Create() => View(new RoleViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RoleViewModel rvm)
        {
            if (!ModelState.IsValid) return View(rvm);
            if (rvm.Name == AppConstants.SUPER_ADMIN_ROLE)
            {
                ModelState.AddModelError("Name", "Cannot create this role");
                return View(rvm);
            }

            var role = await roleService.GetRoleByNameAsync(rvm.Name);
            if (role != null)
            {
                ModelState.AddModelError("Name", "Role already exists");
                return View(rvm);
            }

            await roleService.CreateRoleAsync(rvm);
            TempData["success"] = "Role created";
            return View(new RoleViewModel());
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var et = await roleService.GetRoleByIdAsync(id);
            if (et == null) return NotFound();
            return View(new RoleViewModel
            {
                Id = id,
                Name = et.Name ?? "Default",
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, RoleViewModel rvm)
        {
            if (!ModelState.IsValid) return View(rvm);
            var r = await roleService.UpdateRoleAsync(id, rvm);
            if (r == null)
            {
                TempData["error"] = "Role not found";
                return View();
            }
            TempData["success"] = "Role updated";
            return View(new RoleViewModel
            {
                Id = r.Id,
                Name = r.Name ?? "",
                Description = r.Description
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await roleService.DeleteRoleByIdAsync(id);
                TempData["success"] = "Successfully removed the role";
            } catch (KeyNotFoundException ex)
            {
                TempData["error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
