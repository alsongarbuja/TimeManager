using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using TimeManager.Backend.Common;
using TimeManager.Backend.Services;
using TimeManager.Backend.Utility;
using TimeManager.Backend.ViewModels;
using U = TimeManager.Backend.Models.AuthManagement.User;

namespace TimeManager.Backend.Controllers
{
    public class AuthController(
        SignInManager<U> signInManager,
        UserManager<U> userManager,
        ILogger<U> logger,
        IEmployeeService employeeService
        ) : Controller
    {
        [AllowAnonymous]
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
            return View(new SelectProfileViewModel { Profiles = profiles ?? [] });
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
            HttpContext.Session.SetInt32("JobProfileId", model.SelectedProfileId);
            HttpContext.Session.Remove("PendingProfiles");

            return LocalRedirect("/app/dashboard");
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                logger.LogWarning("Login failed: Model validation failed");
                ModelValidationLog.LogModelStateValidationFailedLogs(logger, ModelState);
                return View(model);
            }

            var result = await signInManager.PasswordSignInAsync(
                model.UserName, model.Password, model.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                var user = await userManager.FindByNameAsync(model.UserName);
                if (user == null)
                {
                    TempData["error"] = "User not found";
                    return View(model);
                }
                var role = await userManager.GetRolesAsync(user);

                if (role.Contains(AppConstants.SUPER_ADMIN_ROLE))
                {
                    HttpContext.Session.Remove("DepartmentId");
                    // TODO: Change to dashboard later
                    return LocalRedirect(returnUrl ?? "/app/report"); 
                }

                if (role.Contains(AppConstants.ADMIN_ROLE))
                {
                    var employee = await employeeService.GetEmployeeByUserIdAsync(user.Id);
                    if (employee == null)
                    {
                        TempData["error"] = "Employee data was not found for the User";
                        return View(model);
                    }
                    HttpContext.Session.SetInt32("DepartmentId", employee.DepartmentId ?? 0);
                    // TODO: Change to dashboard later
                    return LocalRedirect(returnUrl ?? "/app/report");
                }

                var profiles = (await employeeService.GetJobProfilesByUserIdAsync(user.Id)).ToList();
                if (profiles.Count == 1)
                {
                    HttpContext.Session.SetInt32("DepartmentId", profiles[0].ProfileTemplate.Unit.DepartmentId);
                    HttpContext.Session.SetInt32("JobProfileId", profiles[0].Id);
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
            await signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied() => View();
    }
}
