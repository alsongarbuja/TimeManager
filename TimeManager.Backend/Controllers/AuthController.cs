using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using TimeManager.Backend.Services;
using TimeManager.Backend.ViewModels;
using U = TimeManager.Backend.Models.AuthManagement.User;

namespace TimeManager.Backend.Controllers
{
    [AllowAnonymous]
    public class AuthController : Controller
    {
        private readonly SignInManager<U> _signInManager;
        private readonly UserManager<U> _userManager;
        private readonly IEmployeeService _employeeService;
        private readonly IDepartmentService departmentService;

        public AuthController(
            SignInManager<U> signInManager, 
            UserManager<U> userManager,
            IDepartmentService departmentService,
            IEmployeeService employeeService
        )
        {
            _signInManager = signInManager;
            _userManager = userManager;
            this.departmentService = departmentService;
            _employeeService = employeeService;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpGet]
        [Authorize]
        public IActionResult SelectProfile()
        {
            var json = HttpContext.Session.GetString("PendingProfiles");

            if (string.IsNullOrEmpty(json))
                return RedirectToAction("Index", "Home");

            var profiles = JsonSerializer.Deserialize<List<ProfileSelectionItem>>(json);
            return View(new SelectProfileViewModel { Profiles = profiles });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SelectProfile(SelectProfileViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var json = HttpContext.Session.GetString("PendingProfiles");

            if (string.IsNullOrEmpty(json))
                return RedirectToAction("Login", "Account");

            var profiles = JsonSerializer.Deserialize<List<ProfileSelectionItem>>(json);
            var selected = profiles?.FirstOrDefault(p => p.Id == model.SelectedProfileId);

            if (selected == null)
            {
                ModelState.AddModelError("", "Invalid profile selection.");
                return View(model);
            }

            HttpContext.Session.SetInt32("DepartmentId", selected.DepartmentId);
            HttpContext.Session.Remove("PendingProfiles");

            return LocalRedirect("/app/dashboard");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                model.UserName, model.Password, model.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByNameAsync(model.UserName);
                var role = await _userManager.GetRolesAsync(user);

                if (role.Contains("SuperAdmin"))
                {
                    HttpContext.Session.Remove("DepartmentId");
                    return LocalRedirect(returnUrl ?? "/app/dashboard");
                }

                if (role.Contains("Admin"))
                {
                    var employee = await _employeeService.GetEmployeeByUserIdAsync(user.Id);
                    HttpContext.Session.SetInt32("DepartmentId", employee.DepartmentId);
                    return LocalRedirect(returnUrl ?? "/app/dashboard");
                }

                var profiles = (await _employeeService.GetJobProfilesByUserIdAsync(user.Id)).ToList();
                if (profiles.Count == 1)
                {
                    HttpContext.Session.SetInt32("DepartmentId", profiles[0].ProfileTemplate.Unit.DepartmentId);
                    return LocalRedirect(returnUrl ?? "/app/dashboard");
                }

                if (profiles.Count > 1)
                {
                    HttpContext.Session.SetString("PendingProfiles", JsonSerializer.Serialize(
                profiles.Select(p => new { p.Id, Title = $"{p.ProfileTemplate.Role.Name} / {p.ProfileTemplate.Unit.Name}", DepartmentName = p.ProfileTemplate.Unit.Department.Name, p.ProfileTemplate.Unit.DepartmentId })));
                    return RedirectToAction("SelectProfile", "Auth");
                }

                ModelState.AddModelError("", "No Job profile found associated with this account. Contact your administrator.");
                return View(model);
            }

            if (result.IsLockedOut)
                ModelState.AddModelError("", "Account locked. Try again later.");
            else
                ModelState.AddModelError("", "Invalid email or password.");

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied() => View();
    }
}
