using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TimeManager.Backend.Models.Requests;
using TimeManager.Backend.Models.Responses;
using TimeManager.Backend.Services;
using TimeManager.Backend.ViewModels;
using U = TimeManager.Backend.Models.AuthManagement.User;

namespace TimeManager.Backend.Controllers.User
{
    [Authorize(Roles = "SuperAdmin")]
    public class UserController(UserManager<U> userManager, IUserService userService, IRoleService roleService, IConfiguration configuration) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] PaginationFilter filter)
        {
            PagedResponse<UserViewModel> users = await userService.GetUsersAsync(filter);
            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new RegisterViewModel
            {
                AvailableRoles = (await roleService.GetRoleOptionsAsync())
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RegisterViewModel rvm)
        {
            if (!ModelState.IsValid)
            {
                return View(new RegisterViewModel {
                    Email = rvm.Email,
                    Role = rvm.Role,
                    Password = rvm.Password,
                    ConfirmPassword = rvm.ConfirmPassword,
                    AvailableRoles = (await roleService.GetRoleOptionsAsync(rvm.Role))
                });
            }

            var user = new U { UserName = rvm.Email.Split("@")[0], Email = rvm.Email, EmailConfirmed = true };
            var defaultPassword = configuration["Auth:DefaultPassword"] ?? throw new InvalidOperationException("Default password is not configured in the env");
            var toUserPassword = rvm.Password ?? defaultPassword;
            var result = await userManager.CreateAsync(user, toUserPassword);

            if (result.Succeeded)
            {
                try
                {
                    var role = await roleService.GetRoleByIdAsync(rvm.Role) ?? throw new KeyNotFoundException("Role not found for the given Id");
                    await userManager.AddToRoleAsync(user, role.Name!);
                    return RedirectToAction(nameof(Index));
                } catch (KeyNotFoundException ex)
                {
                    TempData["error"] = ex.Message;
                }
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.Code, error.Description);
            }

            return View(new RegisterViewModel
            {
                Email = rvm.Email,
                Role = rvm.Role,
                Password = rvm.Password,
                ConfirmPassword = rvm.ConfirmPassword,
                AvailableRoles = (await roleService.GetRoleOptionsAsync(rvm.Role))
            });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var (user, role) = await userService.GetUserByIdAsync(id);

            if (user == null || role == null) {
                throw new KeyNotFoundException("User or Role is not found");
            }

            var model = new RegisterViewModel
            {
                Id = user.Id,
                Email = user.Email ?? "",
                Role = role.Id,
                AvailableRoles = (await roleService.GetRoleOptionsAsync(role.Id))
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, RegisterViewModel rvm)
        {
            var u = await userService.UpdateUserAsync(id, rvm);
            if (u == null) return View(rvm);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await userService.DeleteUserByIdAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
