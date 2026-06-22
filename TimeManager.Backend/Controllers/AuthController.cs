using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TimeManager.Backend.Services;
using TimeManager.Backend.ViewModels;
using U = TimeManager.Backend.Models.AuthManagement.User;

namespace TimeManager.Backend.Controllers
{
    [AllowAnonymous]
    public class AuthController : Controller
    {
        private readonly SignInManager<U> _signInManager;
        private readonly IDepartmentService departmentService;

        public AuthController(SignInManager<U> signInManager, IDepartmentService departmentService)
        {
            _signInManager = signInManager;
            this.departmentService = departmentService;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                //var errors = ModelState.Where(x => x.Value.Errors.Count > 0)
                //           .Select(x => new {
                //               Key = x.Key,
                //               Errors = string.Join(", ", x.Value.Errors.Select(e => e.ErrorMessage))
                //           });

                //foreach (var error in errors)
                //{
                //    Console.WriteLine($"Field: {error.Key} | Error: {error.Errors}");
                //}
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                model.UserName, model.Password, model.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
                return LocalRedirect(returnUrl ?? "/app/dashboard");

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
