using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using TimeManager.Backend.Common;
using TimeManager.Backend.Services;
using TimeManager.Backend.ViewModels;
using U = TimeManager.Backend.Models.AuthManagement.User;

namespace TimeManager.Backend.Controllers
{
    public class SettingController(UserManager<U> userManager, ICacheService cacheService) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var model = new SettingViewModel();

            if (User.IsInRole(AppConstants.ADMIN_ROLE) || User.IsInRole(AppConstants.SUPER_ADMIN_ROLE))
            {
                var userClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (int.TryParse(userClaim, out int userId))
                {
                    model.Preferences = await cacheService.GetPreferencesAsync(userId);
                }
            }

            PopulateDropDown(model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(SettingViewModel model)
        {
            ChangePasswordModel cpm = model.PasswordModel;

            if (!ModelState.IsValid)
            {
                return await RebuildIndexViewAndResult(model);
            }

            var user = await userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound("User not found. Please contact administration");
            }

            var isPasswordChanged = await userManager.ChangePasswordAsync(user, cpm.OldPassword, cpm.NewPassword);

            if (isPasswordChanged.Succeeded)
            {
                TempData["success"] = "Successfully updated the password. Please use new password to login from now";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in isPasswordChanged.Errors)
            {
                if (error.Code == "PasswordMismatch")
                {
                    ModelState.AddModelError("OldPassword", "The old password you entered is incorrect.");
                }
                else
                {
                    ModelState.AddModelError("NewPassword", error.Description);
                }
            }

            return await RebuildIndexViewAndResult(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePreferences(SettingViewModel model)
        {
            var userClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(userClaim, out int userId))
            {
                await cacheService.UpdatePreferencesAsync(userId, model.Preferences);
                TempData["success"] = "Successfully updated the preferences.";
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<IActionResult> RebuildIndexViewAndResult(SettingViewModel model)
        {
            if (User.IsInRole(AppConstants.ADMIN_ROLE))
            {
                var userClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (int.TryParse(userClaim, out int userId))
                {
                    model.Preferences = await cacheService.GetPreferencesAsync(userId);
                }
            }

            return View("Index", model);
        }

        private void PopulateDropDown(SettingViewModel model) {
            model.LimitOptions = new List<SelectListItem> { 
                new SelectListItem { Text = "5", Value = "5" },
                new SelectListItem { Text = "10", Value = "10" },
                new SelectListItem { Text = "25", Value = "25" },
                new SelectListItem { Text = "50", Value = "50" },
            };

            model.PunchesOrderByOptions = new List<SelectListItem> {
                new SelectListItem { Text = "Employee Name", Value = "name" },
                new SelectListItem { Text = "Clock in", Value = "clock in" },
                new SelectListItem { Text = "Clock out", Value = "clock out" },
            };

            model.EmployeesOrderByOptions = new List<SelectListItem> {
                new SelectListItem { Text = "Employee Name", Value = "name" },
                new SelectListItem { Text = "SO ID", Value = "so id" },
            };

            model.JobProfilesOrderByOptions = new List<SelectListItem> {
                new SelectListItem { Text = "Employee Name", Value = "employee" },
                new SelectListItem { Text = "Unit", Value = "profile template" },
            };
        }
    }
}
