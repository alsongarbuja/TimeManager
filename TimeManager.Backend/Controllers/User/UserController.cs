using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TimeManager.Backend.Services;
using TimeManager.Backend.ViewModels;
using U = TimeManager.Backend.Models.AuthManagement.User;

namespace TimeManager.Backend.Controllers.User
{
    [Authorize(Roles = "SuperAdmin")]
    public class UserController : Controller
    {
        private readonly UserManager<U> _userManager;
        private readonly IRoleService roleService;
        private readonly IUserService userService;
        private readonly IConfiguration _configuration;

        public UserController(UserManager<U> userManager, IUserService userService, IRoleService roleService, IConfiguration configuration)
        {
            _userManager = userManager;
            this.roleService = roleService;
            this.userService = userService;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var users = await this.userService.GetUsersAsync();
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
                return View(rvm);
            }

            var user = new U { UserName = rvm.Email.Split("@")[0], Email = rvm.Email, EmailConfirmed = true };
            var defaultPassword = _configuration["Auth:DefaultPassword"] ?? throw new InvalidOperationException("Default password is not configured in the env");
            var toUserPassword = rvm.Password ?? defaultPassword;
            var result = await _userManager.CreateAsync(user, toUserPassword);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, rvm.Role);
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(rvm);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var (user, role) = await userService.GetUserByIdAsync(id);

            var model = new RegisterViewModel
            {
                Id = user.Id,
                Email = user.Email,
                Role = role[0],
                AvailableRoles = (await roleService.GetRoleOptionsAsync(role[0]))
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
    }
}
