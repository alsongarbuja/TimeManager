using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TimeManager.Backend.Extensions;
using U = TimeManager.Backend.Models.AuthManagement.User;
using TimeManager.Backend.Services;
using TimeManager.Backend.ViewModels;
using TimeManager.Backend.Common;

namespace TimeManager.Backend.Controllers
{
    public class DashboardController(
        IDashboardService dashboardService,
        UserManager<U> userManager
    ) : Controller
    {
        public async Task<IActionResult> Index()
        {
            int? jobProfileId = HttpContext.Session.GetCurrentUserJobProfileId();
            var data = await dashboardService.GetCurrentUserDashboardData(jobProfileId ?? 0);
            return View(data);
        }

        [HttpGet]
        public async Task<IActionResult> Setup()
        {
            var data = await dashboardService.GetSuperAdminSetupDashboardCheckData();
            return View(data);
        }

        [HttpGet]
        public async Task<IActionResult> SuperAdminAdd()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SuperAdminAdd(RegisterSuperAdminModel rsvm)
        {
            if (!ModelState.IsValid)
            {
                return View(new RegisterSuperAdminModel
                {
                    Email = rsvm.Email,
                    Password = rsvm.Password,
                    ConfirmPassword = rsvm.ConfirmPassword,
                });
            }

            var user = new U { UserName = rsvm.Email.Split("@")[0], Email = rsvm.Email, EmailConfirmed = true };
            var result = await userManager.CreateAsync(user, rsvm.Password!);

            if (result.Succeeded)
            {
                try
                {
                    await userManager.AddToRoleAsync(user, AppConstants.SUPER_ADMIN_ROLE);
                    TempData["success"] = "User added successfully";
                    return RedirectToAction(nameof(Setup));
                }
                catch (KeyNotFoundException ex)
                {
                    TempData["error"] = ex.Message;
                }
            }

            return RedirectToAction(nameof(Setup));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveDefaultSA()
        {
            await dashboardService.RemoveDefaultSuperAdmin();
            
            return RedirectToAction(nameof(Setup));
        }
    }
}
