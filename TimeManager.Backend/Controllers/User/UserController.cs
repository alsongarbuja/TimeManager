using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TimeManager.Backend.Services;
using TimeManager.Backend.ViewModels;
using R = TimeManager.Backend.Models.AuthManagement.Role;
using U = TimeManager.Backend.Models.AuthManagement.User;

namespace TimeManager.Backend.Controllers.User
{
    public class UserController : Controller
    {
        private readonly UserManager<U> _userManager;
        private readonly RoleManager<R> _roleManager;
        private readonly IUserService userService;
        private readonly IConfiguration _configuration;

        public UserController(UserManager<U> userManager, IUserService userService, RoleManager<R> roleManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
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
                AvailableRoles = _roleManager.Roles
            .Select(r => new SelectListItem { Value = r.Name, Text = r.Name })
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
    }
}
