using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeManager.Backend.Common;
using TimeManager.Backend.Services;
using TimeManager.Backend.ViewModels;

namespace TimeManager.Backend.Controllers.Role
{
    [Authorize(Roles = "SuperAdmin")]
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
        public async Task<IActionResult> Create(RoleViewModel evm)
        {
            if (!ModelState.IsValid) return View(evm);
            if (evm.Name == AppConstants.SUPER_ADMIN_ROLE)
            {
                ModelState.AddModelError("Name", "Cannot create this role");
                return View(evm);
            }

            var role = await roleService.GetRoleByNameAsync(evm.Name);
            if (role != null)
            {
                ModelState.AddModelError("Name", "Role already exists");
                return View(evm);
            }

            await roleService.CreateRoleAsync(new RoleDto
            {
                Name = evm.Name,
                Description = evm.Description,
            });
            TempData["success"] = "Role created";
            return RedirectToAction(nameof(Index));
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
        public async Task<IActionResult> Edit(int id, RoleViewModel evm)
        {
            if (!ModelState.IsValid) return View(evm);
            var et = await roleService.UpdateRoleAsync(id, new RoleDto
            {
                Name = evm.Name,
                Description = evm.Description,
            });
            if (et == null)
            {
                TempData["error"] = "Role not found";
                return View();
            }
            TempData["success"] = "Role updated";
            return RedirectToAction(nameof(Index));
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
